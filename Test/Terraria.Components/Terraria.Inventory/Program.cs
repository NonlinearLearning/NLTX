using System.Numerics;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Entities;
using Terraria.Share.Inventory;
using Terraria.Share.Items;
using Terraria.Unshared.Player;
using Terraria.Share.Relationships;

// 玩家库存双模型消除后的契约验证。
// 验证类型：contract（单点 API 契约）+ chain（玩家输入 → 共享库存的链路）。
//
// 背景：Share/Inventory 原本只有实体式的 TryInsert/TryRemove/TryMove（要求物品先是带
// ItemStackComponent 的 ECS 实体），而协议路径是槽位赋值式的（包 5 直接给出
// itemType/stack/prefix），没有物品实体可搬。为此给 InventorySystem 增加了
// TrySetSlotValue，并把 Player 域原本自带的 PlayerInventoryComponent 迁移过来删除。
//
// 本验证固定三件事：
//   阶段 1 —— TrySetSlotValue 自身的契约（成功、覆盖、清空、每类拒绝、拒绝不留部分写入）。
//   阶段 2 —— 迁移后的端到端路径（PlayerInputRequestSystem 经 TrySetSlotValue 写入）。
//   阶段 3 —— 容器只承载值（双宿主边界的容器侧，见
//             docs/plans/2026-11-23-item-payload-dual-host-execution-plan.md INV-1/INV-3）。
int failures = 0;

void Check(string name, bool ok, string detail)
{
  Console.WriteLine($"{(ok ? "PASS" : "FAIL")} {name} | {detail}");
  if (!ok)
  {
    failures++;
  }
}

const int BigMaxStack = 9999;

// bigItem 取 General 分类，才能与 Weapon-only 槽位真正不相交；smallItem 只给 5 格堆叠上限。
ItemId bigItem = ItemId.Mushroom;
ItemId smallItem = Enum.GetValues<ItemId>().First(id => id != ItemId.Mushroom && id != ItemId.None);

// ---- 阶段 1：TrySetSlotValue 契约 ----
InventorySlotDefinition[] contractSlots =
[
    new(0, InventorySlotKind.General, ItemCategoryMask.Any, BigMaxStack),
    new(1, InventorySlotKind.General, ItemCategoryMask.Any, 20),
    new(2, InventorySlotKind.General, ItemCategoryMask.Weapon, BigMaxStack),
    new(3, InventorySlotKind.General, ItemCategoryMask.Any, BigMaxStack)
];

ItemRegistry registry = new();
registry.Register(new Item(bigItem, BigMaxStack, ItemCategoryMask.General));
registry.Register(new Item(smallItem, 5, ItemCategoryMask.Any));

using (ArchWorld world = ArchWorld.Create())
{
  InventorySystem inventories = new(world, registry);
  ArchEntity holder = world.Create(new InventoryComponent(contractSlots));
  // 无库存组件的实体，用于验证 MissingInventory。
  ArchEntity bare = world.Create();

  InventoryTransferResult write = inventories.TrySetSlotValue(holder, 0, new ItemStack(bigItem, 7));
  _ = world.Get<InventoryComponent>(holder).TryGetSlot(0, out InventorySlot slot0);
  Check(
      "set-valid",
      write.Succeeded && write.Quantity == 7 &&
          slot0.Content is { } c0 && c0.ItemId == bigItem && c0.Quantity == 7,
      $"succeeded={write.Succeeded} qty={write.Quantity} content={Describe(slot0)}");

  // 覆盖写入：整格替换，不与既有内容合并
  InventoryTransferResult overwrite =
      inventories.TrySetSlotValue(holder, 0, new ItemStack(bigItem, 3));
  _ = world.Get<InventoryComponent>(holder).TryGetSlot(0, out InventorySlot slot0b);
  Check(
      "overwrite-replaces",
      overwrite.Succeeded && slot0b.Content is { } c0b && c0b.Quantity == 3,
      $"succeeded={overwrite.Succeeded} content={Describe(slot0b)}");

  // 用空堆清空槽位
  InventoryTransferResult clear = inventories.TrySetSlotValue(holder, 0, default);
  _ = world.Get<InventoryComponent>(holder).TryGetSlot(0, out InventorySlot slot0c);
  Check(
      "clear-with-empty-stack",
      clear.Succeeded && clear.Quantity == 0 && slot0c.IsEmpty,
      $"succeeded={clear.Succeeded} qty={clear.Quantity} isEmpty={slot0c.IsEmpty}");

  InventoryTransferResult clearAgain = inventories.TrySetSlotValue(holder, 3, default);
  Check(
      "clear-empty-slot",
      clearAgain.Succeeded && clearAgain.Quantity == 0,
      $"succeeded={clearAgain.Succeeded} qty={clearAgain.Quantity}");

  InventoryTransferResult outOfRange =
      inventories.TrySetSlotValue(holder, 99, new ItemStack(bigItem, 1));
  Check(
      "reject-slot-out-of-range",
      !outOfRange.Succeeded && outOfRange.Failure == InventoryTransferFailure.InvalidSlot,
      $"failure={outOfRange.Failure}");

  InventoryTransferResult negative =
      inventories.TrySetSlotValue(holder, -1, new ItemStack(bigItem, 1));
  Check(
      "reject-negative-slot",
      !negative.Succeeded && negative.Failure == InventoryTransferFailure.InvalidSlot,
      $"failure={negative.Failure}");

  // 物品在枚举里已知，但注册表没有它的定义
  ItemId unregistered = Enum.GetValues<ItemId>()
      .First(id => id != bigItem && id != smallItem && id != ItemId.None);
  InventoryTransferResult missingDefinition =
      inventories.TrySetSlotValue(holder, 0, new ItemStack(unregistered, 1));
  _ = world.Get<InventoryComponent>(holder).TryGetSlot(0, out InventorySlot afterMissing);
  Check(
      "reject-missing-definition",
      !missingDefinition.Succeeded &&
          missingDefinition.Failure == InventoryTransferFailure.MissingItemDefinition &&
          afterMissing.IsEmpty,
      $"failure={missingDefinition.Failure} slotStillEmpty={afterMissing.IsEmpty}");

  InventoryTransferResult overSlotLimit =
      inventories.TrySetSlotValue(holder, 1, new ItemStack(bigItem, 21));
  Check(
      "reject-over-slot-max-stack",
      !overSlotLimit.Succeeded && overSlotLimit.Failure == InventoryTransferFailure.InvalidQuantity,
      $"failure={overSlotLimit.Failure}");

  // 超过物品自身堆叠上限：由 Item.AcceptsQuantity 拒绝，报 InvalidItemStack
  InventoryTransferResult overItemLimit =
      inventories.TrySetSlotValue(holder, 0, new ItemStack(smallItem, 6));
  _ = world.Get<InventoryComponent>(holder).TryGetSlot(0, out InventorySlot afterItemLimit);
  Check(
      "reject-over-item-max-stack",
      !overItemLimit.Succeeded && overItemLimit.Failure == InventoryTransferFailure.InvalidItemStack &&
          afterItemLimit.IsEmpty,
      $"failure={overItemLimit.Failure} slotStillEmpty={afterItemLimit.IsEmpty}");

  InventoryTransferResult wrongCategory =
      inventories.TrySetSlotValue(holder, 2, new ItemStack(bigItem, 1));
  Check(
      "reject-category-mismatch",
      !wrongCategory.Succeeded && wrongCategory.Failure == InventoryTransferFailure.NoCompatibleSlot,
      $"failure={wrongCategory.Failure}");

  InventoryTransferResult noInventory = inventories.TrySetSlotValue(bare, 0, new ItemStack(bigItem, 1));
  Check(
      "reject-missing-inventory",
      !noInventory.Succeeded && noInventory.Failure == InventoryTransferFailure.MissingInventory,
      $"failure={noInventory.Failure}");

  InventoryTransferResult exactLimit = inventories.TrySetSlotValue(holder, 1, new ItemStack(bigItem, 20));
  Check(
      "accept-exact-max-stack",
      exactLimit.Succeeded && exactLimit.Quantity == 20,
      $"succeeded={exactLimit.Succeeded} qty={exactLimit.Quantity}");
}

// ---- 阶段 2：迁移后的玩家输入路径 ----
using (ArchWorld inputWorld = ArchWorld.Create())
{
  ArchEntity queueEntity = inputWorld.Create(new PlayerInputRequestQueueComponent());
  PlayerInputRequestQueueComponent queue =
      inputWorld.Get<PlayerInputRequestQueueComponent>(queueEntity);
  EntityRuntimeRegistry entities = new(worldScope: 1);
  EntityRuntimeHandle playerHandle = entities.Allocate(EntityUuid.Create());
  ArchEntity player = inputWorld.Create(
      new LocationComponent(new Vector2(0f, 0f)),
      new InventoryComponent(GeneralSlots(4)));
  entities.Register(playerHandle, player);

  ItemRegistry inputDefinitions = new();
  inputDefinitions.Register(new Item(bigItem, BigMaxStack, ItemCategoryMask.General));
  InventorySystem inputInventories = new(inputWorld, inputDefinitions);
  PlayerInputRequestSystem system = new(inputWorld, entities, inputInventories);

  queue.Enqueue(new PlayerInputRequest(
      PlayerInputRequestKind.SetPosition,
      playerHandle,
      Position: new Vector2(100f, 50f)));
  queue.Enqueue(new PlayerInputRequest(
      PlayerInputRequestKind.SetInventorySlot,
      playerHandle,
      Slot: 1,
      ItemType: (int)bigItem,
      Stack: 7));
  // 未知玩家必须被跳过，而不是抛异常。
  queue.Enqueue(new PlayerInputRequest(
      PlayerInputRequestKind.SetPosition,
      entities.Allocate(EntityUuid.Create()),
      Position: new Vector2(-1f, -1f)));
  system.Update(new EcsServerTickContext(inputWorld, 0, 1f / 60f));

  Vector2 appliedPosition = inputWorld.Get<LocationComponent>(player).Coordinates;
  Check(
      "player-input-set-position",
      appliedPosition == new Vector2(100f, 50f),
      $"position=({appliedPosition.X},{appliedPosition.Y})");

  _ = inputWorld.Get<InventoryComponent>(player).TryGetSlot(1, out InventorySlot written);
  Check(
      "player-input-writes-shared-inventory",
      written.Content is { } cw && cw.ItemId == bigItem && cw.Quantity == 7,
      $"content={Describe(written)}");
  Check("player-input-consumed", queue.Count == 0, $"left={queue.Count}");

  // ItemType<=0 必须按空槽处理（对齐迁移前 PlayerInventorySlot.IsEmpty 的语义）。
  queue.Enqueue(new PlayerInputRequest(
      PlayerInputRequestKind.SetInventorySlot,
      playerHandle,
      Slot: 1,
      ItemType: 0,
      Stack: 0));
  system.Update(new EcsServerTickContext(inputWorld, 1, 1f / 60f));
  _ = inputWorld.Get<InventoryComponent>(player).TryGetSlot(1, out InventorySlot cleared);
  Check(
      "player-input-clears-on-zero-item-type",
      cleared.IsEmpty,
      $"isEmpty={cleared.IsEmpty}");
}
// ---- 阶段 3：容器只承载值（双宿主边界的容器侧） ----
// 对照 docs/plans/2026-11-23-item-payload-dual-host-execution-plan.md INV-1/INV-3。
// 容器宿主与世界宿主互斥的必要条件是：容器槽只存 ItemStack 值，不持有第二个物品身份。
// 这组断言按行为口径写，不绑定具体成员名——槽位若将来去掉实体引用表达，
// 这里仍然应当保持 PASS，而不是因为缺少某个属性而编译失败。
using (ArchWorld valueWorld = ArchWorld.Create())
{
  ArchEntity valueHolder = valueWorld.Create(new InventoryComponent(GeneralSlots(2)));
  ItemRegistry valueDefinitions = new();
  valueDefinitions.Register(new Item(bigItem, BigMaxStack, ItemCategoryMask.General));
  InventorySystem valueInventories = new(valueWorld, valueDefinitions);

  _ = valueWorld.Get<InventoryComponent>(valueHolder).TryGetSlot(0, out InventorySlot emptySlot);
  Check(
      "slot-empty-has-no-content",
      emptySlot.IsEmpty && emptySlot.Content is null,
      $"isEmpty={emptySlot.IsEmpty} content={(emptySlot.Content is null ? "none" : "present")}");

  InventoryTransferResult valueWrite =
      valueInventories.TrySetSlotValue(valueHolder, 0, new ItemStack(bigItem, 9));
  _ = valueWorld.Get<InventoryComponent>(valueHolder).TryGetSlot(0, out InventorySlot valueSlot);
  Check(
      "slot-value-carries-stack-only",
      valueWrite.Succeeded && valueSlot.Content is { } storedValue &&
          storedValue.ItemId == bigItem && storedValue.Quantity == 9 && storedValue.Prefix == 0,
      $"content={Describe(valueSlot)}（容器侧只保存载荷三元组）");

  // 值槽必须能被取出，说明它是"值"而不是"实体引用"：实体槽位在取出时要求结果类型不同。
  InventoryTransferResult valueTake = valueInventories.TryRemove(valueHolder, 0, 4, out ItemStack takenValue);
  Check(
      "slot-value-removable-as-stack",
      valueTake.Succeeded && takenValue.Quantity == 4 && takenValue.ItemId == bigItem,
      $"taken={takenValue.Quantity} id={(int)takenValue.ItemId}");
  _ = valueWorld.Get<InventoryComponent>(valueHolder).TryGetSlot(0, out InventorySlot afterTake);
  Check(
      "slot-value-remove-keeps-remainder",
      afterTake.Content is { } remainder && remainder.Quantity == 5,
      $"content={Describe(afterTake)}（9 取 4 余 5）");

  // 清空后必须回到"空槽"，且不残留任何身份引用。
  InventoryTransferResult valueClear = valueInventories.TrySetSlotValue(valueHolder, 0, default);
  _ = valueWorld.Get<InventoryComponent>(valueHolder).TryGetSlot(0, out InventorySlot clearedSlot);
  Check(
      "slot-value-clear-leaves-nothing",
      valueClear.Succeeded && clearedSlot.IsEmpty && clearedSlot.Content is null,
      $"isEmpty={clearedSlot.IsEmpty} content={(clearedSlot.Content is null ? "none" : "present")}");
}
Console.WriteLine();
Console.WriteLine(
    failures == 0
        ? "PASS: single inventory model — TrySetSlotValue contract, player input path and value-only slots."
        : $"FAIL: {failures} check(s) failed.");
return failures == 0 ? 0 : 1;

static InventorySlotDefinition[] GeneralSlots(int count)
{
  InventorySlotDefinition[] slots = new InventorySlotDefinition[count];
  for (int index = 0; index < count; index++)
  {
    slots[index] = new InventorySlotDefinition(
        index,
        InventorySlotKind.General,
        ItemCategoryMask.Any,
        BigMaxStack);
  }

  return slots;
}

static string Describe(InventorySlot slot)
{
  return slot.Content is { } content
      ? $"itemId={(int)content.ItemId} qty={content.Quantity}"
      : "none";
}
