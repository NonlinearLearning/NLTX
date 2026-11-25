using System;
using System.Collections.Generic;
using System.Numerics;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Combat;
using Terraria.Network;
using Terraria.Unshared.Session;
using Terraria.Share.Entities;
using Terraria.Share.Inventory;
using Terraria.Share.Items;
using Terraria.Share.Liquid;
using Terraria.Share.Movement;
using Terraria.Share.Physics;
using Terraria.Share.Relationships;
using Terraria.Unshared.World;

// 世界掉落物的物品身份改由共享 ItemStack 承载（见审计 §32 与 §34）。
// 验证类型：rule（对照 Version4 的身份/时序/拾取规则）+ chain（世界宿主 → 容器宿主的移交链路）。
// 本验证固定四件事：
//   1. 权威状态只接受 ItemId 里的已知类型；未知类型在入口被拒，不会先存下再投影。
//   2. 身份、数量、前缀与协议 itemType 之间双向无损（换模型不改变线上字节）。
//   3. 清除槽位用空 ItemStack 表达，与原来的 Stack = 0 语义等价。
//   4. 双宿主不变量 INV-1..INV-5（载荷唯一 / 宿主互斥 / 单一写者 / 转移守恒 / 瞬态不入档），
//      见 docs/plans/2026-11-23-item-payload-dual-host-execution-plan.md §3。
int failures = 0;

void Check(string name, bool ok, string detail)
{
  Console.WriteLine($"{(ok ? "PASS" : "FAIL")} {name} | {detail}");
  if (!ok)
  {
    failures++;
  }
}

SessionEpoch actor = SessionEpoch.Create(7);
Vector2 position = new(10f, 20f);
Vector2 velocity = new(1f, -1f);

WorldItemStore store = new();

// --- 1. 已知类型创建成功，且身份进入共享 ItemStack ---
bool created = store.TryCreate(
    ItemIdRules.ToNetworkType(ItemId.Torch),
    stack: 5,
    prefix: 3,
    position,
    velocity,
    reservedFor: null,
    out WorldItemState torch);
Check("create-known-applied", created, $"created={created}");

Check(
    "create-carries-item-stack",
    torch.Item.ItemId == ItemId.Torch && torch.Item.Quantity == 5 && torch.Item.Prefix == 3,
    $"id={torch.Item.ItemId} quantity={torch.Item.Quantity} prefix={torch.Item.Prefix}");

Check(
    "network-type-round-trip",
    ItemIdRules.ToNetworkType(torch.Item.ItemId) == 8,
    $"itemType={ItemIdRules.ToNetworkType(torch.Item.ItemId)}（应为 8）");

IReadOnlyList<WorldItemMutation> spawnMutations = store.DrainMutations();
Check(
    "spawn-mutation-kind",
    spawnMutations.Count == 1 && spawnMutations[0].Kind == WorldItemChangeKind.Spawn,
    $"count={spawnMutations.Count} kind={(spawnMutations.Count > 0 ? spawnMutations[0].Kind.ToString() : "none")}");

// --- 2. 未知 / 非法类型在入口被拒 ---
const int UnknownItemType = 6196;  // ItemId 值域上界是 6195
Check(
    "create-unknown-type-rejected",
    !store.TryCreate(UnknownItemType, 5, 0, position, velocity, null, out _),
    $"itemType={UnknownItemType}（不在 ItemId 中）");

Check(
    "create-zero-stack-rejected",
    !store.TryCreate(8, 0, 0, position, velocity, null, out _),
    "stack=0");

Check(
    "create-negative-type-rejected",
    !store.TryCreate(-48, 5, 0, position, velocity, null, out _),
    "itemType=-48（ItemId 里有该常量，但世界掉落物要求 > 0）");

Check(
    "rejections-queue-nothing",
    store.DrainMutations().Count == 0,
    "被拒绝的创建不应产生变更");

// --- 3. 更新路径的校验 ---
short slot = torch.Slot;
Check(
    "update-unknown-type-rejected",
    !store.TryUpdate(slot, actor, UnknownItemType, 4, 3, position, velocity, out _),
    $"itemType={UnknownItemType}");

Check(
    "update-prefix-mismatch-rejected",
    !store.TryUpdate(slot, actor, 8, 4, prefix: 9, position, velocity, out _),
    "prefix 9 与原状态 3 不一致");

Check(
    "update-larger-stack-rejected",
    !store.TryUpdate(slot, actor, 8, 9, 3, position, velocity, out _),
    "stack 9 大于原数量 5");

Check(
    "update-applied",
    store.TryUpdate(slot, actor, 8, 4, 3, position, velocity, out WorldItemState? updated) &&
        updated is not null && updated.Item.Quantity == 4,
    $"quantity={(updated is null ? -1 : updated.Item.Quantity)}");

// --- 4. 清除槽位等价于空 ItemStack ---
bool retired = store.TryUpdate(
    slot,
    actor,
    itemType: 0,
    stack: 0,
    prefix: 0,
    position,
    velocity,
    out WorldItemState? retiredState);
Check(
    "update-zero-stack-retires",
    retired && retiredState is not null && retiredState.Item.IsEmpty,
    $"retired={retired} isEmpty={(retiredState is null ? false : retiredState.Item.IsEmpty)}");

bool sawRetire = false;
foreach (WorldItemMutation mutation in store.DrainMutations())
{
  sawRetire |= mutation.Kind == WorldItemChangeKind.Retire && mutation.Item.Item.IsEmpty;
}
Check("retire-mutation-carries-empty-item", sawRetire, "Retire 变更携带空 ItemStack");

Check(
    "retired-slot-is-free",
    store.TryCreate(ItemIdRules.ToNetworkType(ItemId.Gel), 2, 0, position, velocity, null, out WorldItemState gel) &&
        gel.Slot == slot,
    $"重新占用槽位 {slot}");

// --- 5. 直接退役同样产生空 ItemStack ---
bool directRetire = store.TryRetire(gel.Slot, actor, out WorldItemState? gone);
Check(
    "direct-retire-yields-empty-item",
    directRetire && gone is not null && gone.Item.IsEmpty,
    $"retired={directRetire} isEmpty={(gone is null ? false : gone.Item.IsEmpty)}");

// --- 6. 世界物品 tick 的纯规则（Version4 WorldItem.MoveInWorld / TryCombiningIntoNearbyItems） ---
Vector2 airGravity = WorldItemMotionSolver.Integrate(Vector2.Zero, WorldItemMotionProfile.Air);
Check(
    "motion-air-gravity",
    airGravity == new Vector2(0f, 0.1f),
    $"velocity={airGravity}（Version4 每 tick +0.1）");

Vector2 fallClamp = WorldItemMotionSolver.Integrate(new Vector2(1f, 7f), WorldItemMotionProfile.Air);
Check("motion-air-fall-clamp", fallClamp.Y == 7f, $"velocity.Y={fallClamp.Y}（上限 7）");

Vector2 deadZone = WorldItemMotionSolver.Integrate(new Vector2(0.05f, 0f), WorldItemMotionProfile.Air);
Check("motion-horizontal-dead-zone", deadZone.X == 0f, $"velocity.X={deadZone.X}（±0.1 内归零）");

Check(
    "motion-profile-shimmer",
    WorldItemMotionSolver.ProfileFor(LiquidContactFlags.Shimmer) == WorldItemMotionProfile.Shimmer &&
        WorldItemMotionSolver.ProfileFor(LiquidContactFlags.Honey) == WorldItemMotionProfile.Honey,
    "微光 0.065/4、蜂蜜 0.05/3 优先于水");

Check(
    "motion-liquid-half-speed",
    WorldItemMotionSolver.ToDisplacement(new Vector2(2f, 2f), WorldItemMotionProfile.Water) ==
        new Vector2(1f, 1f),
    "水下位移按 0.5 缩放");

WorldItemState survivor = new(
    0,
    new ItemStack(ItemId.Gel, 5),
    new Vector2(0f, 0f),
    Vector2.Zero,
    null,
    1);
Check(
    "merge-amount",
    WorldItemMergeRules.MergeAmount(survivor, survivor with { Slot = 1, Position = new Vector2(10f, 0f) }, 99) == 5,
    "同身份 10 像素内合并 5");
Check(
    "merge-distance-limit",
    WorldItemMergeRules.MergeAmount(survivor, survivor with { Slot = 1, Position = new Vector2(31f, 0f) }, 99) == 0,
    "曼哈顿距离 31 > 30 不合并");
Check(
    "merge-requires-same-prefix",
    WorldItemMergeRules.MergeAmount(
        survivor,
        survivor with { Slot = 1, Item = new ItemStack(ItemId.Gel, 5, 2) },
        99) == 0,
    "前缀不同不合并");
Check(
    "merge-full-stack-rejected",
    WorldItemMergeRules.MergeAmount(
        survivor with { Item = new ItemStack(ItemId.Gel, 99) },
        survivor with { Slot = 1 },
        99) == 0,
    "目标已满不合并");

Check(
    "pickup-in-range",
    WorldItemPickupRules.IsWithinGrabRange(
        new Vector2(100f, 100f),
        new Vector2(16f, 16f),
        new Vector2(100f, 96f),
        new Vector2(16f, 32f),
        WorldItemPickupRules.DefaultGrabRange),
    "物品外扩 42 像素后与玩家碰撞体重叠");
Check(
    "pickup-out-of-range",
    !WorldItemPickupRules.IsWithinGrabRange(
        new Vector2(1000f, 100f),
        new Vector2(16f, 16f),
        new Vector2(100f, 96f),
        new Vector2(16f, 32f),
        WorldItemPickupRules.DefaultGrabRange),
    "距离足够远时不判定可拾取");
Check(
    "pickup-manhattan-cost",
    WorldItemPickupRules.ManhattanCost(
        new Vector2(100f, 100f),
        new Vector2(16f, 16f),
        new Vector2(104f, 96f),
        new Vector2(16f, 32f)) == 8,
    "中心曼哈顿距离 8");

// --- 7. 存储层的 tick 原语 ---
WorldItemStore opsStore = new();
opsStore.TryCreate(
    ItemIdRules.ToNetworkType(ItemId.Gel),
    5,
    0,
    new Vector2(10f, 10f),
    Vector2.Zero,
    reservedFor: null,
    out WorldItemState first);
opsStore.TryCreate(
    ItemIdRules.ToNetworkType(ItemId.Gel),
    4,
    0,
    new Vector2(20f, 10f),
    Vector2.Zero,
    reservedFor: null,
    out WorldItemState second);
opsStore.DrainMutations();

bool motionWritten = opsStore.TrySetMotion(
    first.Slot,
    new Vector2(11f, 12f),
    new Vector2(0f, 1f),
    out WorldItemState? movedItem);
Check(
    "motion-write-does-not-emit-mutation",
    motionWritten && movedItem is not null && opsStore.DrainMutations().Count == 0,
    "运动每 tick 写入但不产生 SyncItem 变更");

bool combined = opsStore.TryCombineStacks(
    first.Slot,
    second.Slot,
    amount: 4,
    out WorldItemState? combinedSurvivor,
    out WorldItemState? combinedDrained);
Check(
    "combine-adds-quantity",
    combined && combinedSurvivor is not null && combinedSurvivor.Item.Quantity == 9,
    $"quantity={(combinedSurvivor is null ? -1 : combinedSurvivor.Item.Quantity)}");
Check(
    "combine-retires-drained-slot",
    combinedDrained is not null && combinedDrained.Item.IsEmpty &&
        !opsStore.TryGet(second.Slot, out _),
    "被合并的槽位清空");

bool partialConsume = opsStore.TryConsume(first.Slot, default, quantity: 4, out WorldItemState? afterConsume);
Check(
    "consume-partial",
    partialConsume && afterConsume is not null && afterConsume.Item.Quantity == 5,
    $"quantity={(afterConsume is null ? -1 : afterConsume.Item.Quantity)}");

bool fullConsume = opsStore.TryConsume(first.Slot, default, quantity: 5, out WorldItemState? consumed);
Check(
    "consume-retires-empty-slot",
    fullConsume && consumed is not null && !opsStore.TryGet(first.Slot, out _),
    "取空后槽位释放");

WorldItemState[] snapshot = opsStore.SnapshotItems();
Check("snapshot-is-sorted", snapshot.Length == 0 || snapshot[0].Slot <= snapshot[snapshot.Length - 1].Slot, $"count={snapshot.Length}");

// --- 8. 集成：下落、落地与静止（无会话，不触发拾取） ---
using ArchWorld fallWorld = ArchWorld.Create();
WorldRuntime fallRuntime = BuildFloorRuntime(worldWidth: 30, worldHeight: 20, floorRow: 18);
WorldItemStore fallStore = new();
EntityRuntimeRegistry fallEntities = new(worldScope: 1);
fallStore.TryCreate(
    ItemIdRules.ToNetworkType(ItemId.Gel),
    5,
    0,
    new Vector2(48f, 0f),
    Vector2.Zero,
    reservedFor: null,
    out WorldItemState fallingItem);
fallStore.DrainMutations();
WorldItemTickSystem fallTick = new(
    fallWorld,
    fallStore,
    fallRuntime,
    fallRuntime,
    new SessionPlayerSource(fallWorld, fallEntities),
    maximumStackSize: 99);
for (int tick = 0; tick < 90; tick++)
{
  fallTick.Update(new EcsServerTickContext(fallWorld, tick, 1f / 60f));
}

Check(
    "falling-item-rests-on-floor",
    fallStore.TryGet(fallingItem.Slot, out WorldItemState? landed) &&
        landed is not null &&
        MathF.Abs(landed.Position.Y - 272f) < 0.01f &&
        landed.Velocity == Vector2.Zero,
    $"position={(landed is null ? "none" : landed.Position.ToString())}（楼层顶面 288，物品高 16）");
Check(
    "resting-item-stops-writing",
    fallStore.DrainMutations().Count == 0,
    "整段下落与静止没有产生网络变更");

// --- 9. 集成：同一 tick 内相邻同类物品合并 ---
fallStore.TryCreate(
    ItemIdRules.ToNetworkType(ItemId.Gel),
    5,
    0,
    new Vector2(80f, 272f),
    Vector2.Zero,
    reservedFor: null,
    out WorldItemState mergeLeft);
fallStore.TryCreate(
    ItemIdRules.ToNetworkType(ItemId.Gel),
    7,
    0,
    new Vector2(90f, 272f),
    Vector2.Zero,
    reservedFor: null,
    out WorldItemState mergeRight);
fallStore.DrainMutations();
fallTick.Update(new EcsServerTickContext(fallWorld, 90, 1f / 60f));
Check(
    "nearby-items-merge-same-tick",
    fallStore.TryGet(mergeLeft.Slot, out WorldItemState? mergedLeft) &&
        mergedLeft is not null &&
        mergedLeft.Item.Quantity == 12 &&
        !fallStore.TryGet(mergeRight.Slot, out _),
    $"quantity={(mergedLeft is null ? -1 : mergedLeft.Item.Quantity)}");

// --- 10. 集成：玩家靠近时拾取并写入背包，余量留在世界 ---
using ArchWorld pickupWorld = ArchWorld.Create();
WorldRuntime pickupRuntime = BuildFloorRuntime(worldWidth: 30, worldHeight: 20, floorRow: 18);
_ = pickupWorld.Create(new WorldItemRequestQueueComponent());
_ = pickupWorld.Create(new InventoryPickupRequestQueueComponent());
SessionEpoch pickupEpoch = SessionEpoch.Create(11);
CommunicationSessionComponent sessionComponent = new(pickupEpoch);
_ = pickupWorld.Create(
    sessionComponent,
    new CommunicationInboundMessageBufferComponent(),
    new CommunicationOutboundMessageBufferComponent());
EntityRuntimeRegistry pickupEntities = new(worldScope: 1);
EntityUuid playerId = EntityUuid.Create();
EntityRuntimeHandle playerHandle = pickupEntities.Allocate(playerId);
ArchEntity playerEntity = pickupWorld.Create();
pickupWorld.Add(playerEntity, new EntityIdentityComponent(playerId));
pickupWorld.Add(playerEntity, new LocationComponent(new Vector2(100f, 250f)));
pickupWorld.Add(playerEntity, new ColliderComponent(new Vector2(16f, 32f)));
pickupWorld.Add(playerEntity, new LifeComponent(100, 100));
ItemRegistry pickupDefinitions = new();
pickupDefinitions.Register(new Item(ItemId.Torch, 99, ItemCategoryMask.Any));
InventoryComponent playerInventory = new(
[
  new InventorySlotDefinition(0, InventorySlotKind.General, ItemCategoryMask.Any, 99),
]);
pickupWorld.Add(playerEntity, playerInventory);
pickupEntities.Register(playerHandle, playerEntity);
sessionComponent.BindPlayer(playerHandle);
// 只有已激活（完成握手）的会话才会触发拾取，与"玩家已进入世界"一致。
sessionComponent.TryAdvance(CommunicationSessionStage.Active);

WorldItemStore pickupStore = new();
pickupStore.TryCreate(
    ItemIdRules.ToNetworkType(ItemId.Torch),
    5,
    0,
    new Vector2(100f, 272f),
    Vector2.Zero,
    reservedFor: null,
    out WorldItemState torchItem);
pickupStore.TryCreate(
    ItemIdRules.ToNetworkType(ItemId.Torch),
    120,
    0,
    new Vector2(112f, 272f),
    Vector2.Zero,
    reservedFor: null,
    out WorldItemState overflowItem);
pickupStore.DrainMutations();
InventorySystem inventories = new(pickupWorld, pickupDefinitions);
WorldItemTickSystem pickupTick = new(
    pickupWorld,
    pickupStore,
    pickupRuntime,
    pickupRuntime,
    new SessionPlayerSource(pickupWorld, pickupEntities),
    maximumStackSize: 99);
InventoryPickupRequestSystem pickupRequests = new(pickupWorld, pickupEntities, inventories);
WorldItemRequestSystem worldRequests = new(pickupWorld, pickupStore);
for (int tick = 0; tick < 3; tick++)
{
  EcsServerTickContext pickupContext = new(pickupWorld, tick, 1f / 60f);
  pickupTick.Update(pickupContext);
  pickupRequests.Update(pickupContext);
  worldRequests.Update(pickupContext);
}

Check(
    "pickup-removes-world-item",
    !pickupStore.TryGet(torchItem.Slot, out _),
    "被拾取的槽位退役");
Check(
    "pickup-fills-inventory",
    playerInventory.TryGetSlot(0, out InventorySlot stockedSlot) &&
        stockedSlot.Content is ItemStack storedTorches &&
        storedTorches.ItemId == ItemId.Torch &&
        storedTorches.Quantity == 99,
    $"slot0={(playerInventory.TryGetSlot(0, out InventorySlot s) && s.Content is ItemStack c ? c.Quantity : -1)}（上限 99）");
int overflowRemaining = pickupStore.TryGet(overflowItem.Slot, out WorldItemState? overflow) && overflow is not null
    ? overflow.Item.Quantity
    : 0;
Check(
    "pickup-conserves-quantity",
    overflowRemaining == 120 - 99 + 5,
    $"世界剩余={overflowRemaining}（120 收下 99，另有 5 件已入包后余量应为 26）");

bool sawPickupRetire = false;
foreach (WorldItemMutation mutation in pickupStore.DrainMutations())
{
  sawPickupRetire |= mutation.Kind == WorldItemChangeKind.Retire;
}
Check("pickup-emits-retire-fact", sawPickupRetire, "网络侧据此发送物品消失报文");

// --- 11. 集成：失去支撑的物品离开世界底部后被清理 ---
using ArchWorld voidWorld = ArchWorld.Create();
WorldRuntime voidRuntime = new(30, 20);
WorldItemStore voidStore = new();
EntityRuntimeRegistry voidEntities = new(worldScope: 1);
voidStore.TryCreate(
    ItemIdRules.ToNetworkType(ItemId.Gel),
    1,
    0,
    new Vector2(48f, 300f),
    new Vector2(0f, 7f),
    reservedFor: null,
    out WorldItemState voidItem);
WorldItemTickSystem voidTick = new(
    voidWorld,
    voidStore,
    voidRuntime,
    voidRuntime,
    new SessionPlayerSource(voidWorld, voidEntities),
    maximumStackSize: 99);
for (int tick = 0; tick < 5; tick++)
{
  voidTick.Update(new EcsServerTickContext(voidWorld, tick, 1f / 60f));
}

Check(
    "unsupported-item-leaves-world",
    !voidStore.TryGet(voidItem.Slot, out _),
    "世界底部无支撑时退役（Version4 CheckInWorld）");

// --- 12. 双宿主不变量（INV-1..INV-5） ---
// 对照 docs/plans/2026-11-23-item-payload-dual-host-execution-plan.md §3。
// 这些断言的职责不是验证某条业务规则，而是锁住"载荷唯一 / 宿主互斥 / 单一写者"三条边界：
// 任何后续改动若让世界槽与容器槽同时持有同一份物品、或让只读端口长出写入口、或把世界槽
// 接进存档，必须先让这里变红，而不是等线上出现复制或丢失。
using ArchWorld dualWorld = ArchWorld.Create();
WorldRuntime dualRuntime = BuildFloorRuntime(worldWidth: 30, worldHeight: 20, floorRow: 18);
_ = dualWorld.Create(new WorldItemRequestQueueComponent());
_ = dualWorld.Create(new InventoryPickupRequestQueueComponent());
SessionEpoch dualEpoch = SessionEpoch.Create(13);
CommunicationSessionComponent dualSession = new(dualEpoch);
_ = dualWorld.Create(
    dualSession,
    new CommunicationInboundMessageBufferComponent(),
    new CommunicationOutboundMessageBufferComponent());
EntityRuntimeRegistry dualEntities = new(worldScope: 1);
EntityUuid dualPlayerId = EntityUuid.Create();
EntityRuntimeHandle dualHandle = dualEntities.Allocate(dualPlayerId);
ArchEntity dualPlayer = dualWorld.Create();
dualWorld.Add(dualPlayer, new EntityIdentityComponent(dualPlayerId));
dualWorld.Add(dualPlayer, new LocationComponent(new Vector2(100f, 250f)));
dualWorld.Add(dualPlayer, new ColliderComponent(new Vector2(16f, 32f)));
dualWorld.Add(dualPlayer, new LifeComponent(100, 100));
ItemRegistry dualDefinitions = new();
dualDefinitions.Register(new Item(ItemId.Gel, 99, ItemCategoryMask.Any));
InventoryComponent dualInventory = new(
[
  new InventorySlotDefinition(0, InventorySlotKind.General, ItemCategoryMask.Any, 99),
]);
dualWorld.Add(dualPlayer, dualInventory);
dualEntities.Register(dualHandle, dualPlayer);
dualSession.BindPlayer(dualHandle);
dualSession.TryAdvance(CommunicationSessionStage.Active);

// 前缀非 0 才能证明"身份三元组整体过界"，而不只是数量对得上。
const byte CarriedPrefix = 2;
WorldItemStore dualStore = new();
dualStore.TryCreate(
    ItemIdRules.ToNetworkType(ItemId.Gel),
    40,
    CarriedPrefix,
    new Vector2(100f, 272f),
    Vector2.Zero,
    reservedFor: null,
    out WorldItemState exclusiveItem);
dualStore.DrainMutations();
InventorySystem dualInventories = new(dualWorld, dualDefinitions);
WorldItemTickSystem dualTick = new(
    dualWorld,
    dualStore,
    dualRuntime,
    dualRuntime,
    new SessionPlayerSource(dualWorld, dualEntities),
    maximumStackSize: 99);
InventoryPickupRequestSystem dualPickupRequests = new(dualWorld, dualEntities, dualInventories);
WorldItemRequestSystem dualWorldRequests = new(dualWorld, dualStore);

int WorldQuantity(WorldItemStore source)
{
  int total = 0;
  foreach (WorldItemState item in source.SnapshotItems())
  {
    total += item.Item.Quantity;
  }

  return total;
}

int InventoryQuantity(InventoryComponent inventory)
{
  int total = 0;
  foreach (InventorySlot slot in inventory.Slots)
  {
    if (slot.Content is ItemStack stack)
    {
      total += stack.Quantity;
    }
  }

  return total;
}

int worldBefore = WorldQuantity(dualStore);
int inventoryBefore = InventoryQuantity(dualInventory);
Check(
    "invariant-hosts-exclusive-before-pickup",
    worldBefore == 40 && inventoryBefore == 0,
    $"世界={worldBefore} 背包={inventoryBefore}（拾取前只允许一端持有）");

for (int tick = 0; tick < 3; tick++)
{
  EcsServerTickContext dualContext = new(dualWorld, tick, 1f / 60f);
  dualTick.Update(dualContext);
  dualPickupRequests.Update(dualContext);
  dualWorldRequests.Update(dualContext);
}

int worldAfter = WorldQuantity(dualStore);
int inventoryAfter = InventoryQuantity(dualInventory);
Check(
    "invariant-hosts-exclusive-after-pickup",
    worldAfter + inventoryAfter == worldBefore && worldAfter == 0 && inventoryAfter == 40,
    $"世界={worldAfter} 背包={inventoryAfter} 合计={worldAfter + inventoryAfter}（应为 0 / 40 / 40）");

bool carried = dualInventory.TryGetSlot(0, out InventorySlot carriedSlot) &&
    carriedSlot.Content is ItemStack carriedStack;
Check(
    "invariant-payload-single-source",
    carried && carriedSlot.Content is ItemStack bound &&
        bound.ItemId == exclusiveItem.Item.ItemId &&
        bound.Prefix == exclusiveItem.Item.Prefix &&
        bound.Quantity == exclusiveItem.Item.Quantity,
    $"过界三元组={(carriedSlot.Content is ItemStack s2 ? $"{s2.ItemId}/{s2.Quantity}/{s2.Prefix}" : "none")}；" +
        $"源头={exclusiveItem.Item.ItemId}/{exclusiveItem.Item.Quantity}/{exclusiveItem.Item.Prefix}");

// INV-4：部分成交时世界余量必须等于"原量 − 实收"，且槽位不得被误退休。
// 构造口径：部分成交只在"堆 ≤ 定义 MaxStack、但超过槽容量"时发生
// （InventorySystem.PlanInsert 用 min(槽 MaxStackSize, 定义 MaxStack) 截断）。
// 因此这里给 40 的堆和 20 的槽容量，而不是给一个超过定义上限的大堆——后者会被
// GetDefinition 整堆拒绝，测不到部分路径。
using ArchWorld partialWorld = ArchWorld.Create();
WorldRuntime partialRuntime = BuildFloorRuntime(worldWidth: 30, worldHeight: 20, floorRow: 18);
_ = partialWorld.Create(new WorldItemRequestQueueComponent());
_ = partialWorld.Create(new InventoryPickupRequestQueueComponent());
SessionEpoch partialEpoch = SessionEpoch.Create(17);
CommunicationSessionComponent partialSession = new(partialEpoch);
_ = partialWorld.Create(
    partialSession,
    new CommunicationInboundMessageBufferComponent(),
    new CommunicationOutboundMessageBufferComponent());
EntityRuntimeRegistry partialEntities = new(worldScope: 1);
EntityUuid partialPlayerId = EntityUuid.Create();
EntityRuntimeHandle partialHandle = partialEntities.Allocate(partialPlayerId);
ArchEntity partialPlayer = partialWorld.Create();
partialWorld.Add(partialPlayer, new EntityIdentityComponent(partialPlayerId));
partialWorld.Add(partialPlayer, new LocationComponent(new Vector2(100f, 250f)));
partialWorld.Add(partialPlayer, new ColliderComponent(new Vector2(16f, 32f)));
partialWorld.Add(partialPlayer, new LifeComponent(100, 100));
ItemRegistry partialDefinitions = new();
partialDefinitions.Register(new Item(ItemId.Gel, 99, ItemCategoryMask.Any));
InventoryComponent partialInventory = new(
[
  new InventorySlotDefinition(0, InventorySlotKind.General, ItemCategoryMask.Any, 20),
]);
partialWorld.Add(partialPlayer, partialInventory);
partialEntities.Register(partialHandle, partialPlayer);
partialSession.BindPlayer(partialHandle);
partialSession.TryAdvance(CommunicationSessionStage.Active);
WorldItemStore partialStore = new();
partialStore.TryCreate(
    ItemIdRules.ToNetworkType(ItemId.Gel),
    40,
    0,
    new Vector2(100f, 272f),
    Vector2.Zero,
    reservedFor: null,
    out WorldItemState partialItem);
partialStore.DrainMutations();
InventorySystem partialInventories = new(partialWorld, partialDefinitions);
WorldItemTickSystem partialTick = new(
    partialWorld,
    partialStore,
    partialRuntime,
    partialRuntime,
    new SessionPlayerSource(partialWorld, partialEntities),
    maximumStackSize: 99);
InventoryPickupRequestSystem partialRequests = new(partialWorld, partialEntities, partialInventories);
WorldItemRequestSystem partialWorldRequests = new(partialWorld, partialStore);
for (int tick = 0; tick < 3; tick++)
{
  EcsServerTickContext partialContext = new(partialWorld, tick, 1f / 60f);
  partialTick.Update(partialContext);
  partialRequests.Update(partialContext);
  partialWorldRequests.Update(partialContext);
}

int partialRemaining = partialStore.TryGet(partialItem.Slot, out WorldItemState? partialState) &&
    partialState is not null
  ? partialState.Item.Quantity
  : 0;
int partialAccepted = InventoryQuantity(partialInventory);
Check(
    "invariant-transfer-conservation-partial",
    partialAccepted == 20 && partialRemaining == 40 - 20 && partialRemaining + partialAccepted == 40,
    $"实收={partialAccepted} 世界剩余={partialRemaining} 合计={partialRemaining + partialAccepted}（应为 20 / 20 / 40）");

// INV-4 的边界面：世界堆若超过定义 MaxStack，容器整堆拒绝而不是接收一部分。
// 这不是缺陷，是"载荷数量必须先合法"的守卫；但世界侧并不校验定义上限，
// 因此这条断言同时记录了"超限世界堆会一直不可拾取"这一当前事实。
using ArchWorld oversizedWorld = ArchWorld.Create();
_ = oversizedWorld.Create(new InventoryPickupRequestQueueComponent());
_ = oversizedWorld.Create(new WorldItemRequestQueueComponent());
SessionEpoch oversizedEpoch = SessionEpoch.Create(19);
CommunicationSessionComponent oversizedSession = new(oversizedEpoch);
_ = oversizedWorld.Create(
    oversizedSession,
    new CommunicationInboundMessageBufferComponent(),
    new CommunicationOutboundMessageBufferComponent());
EntityRuntimeRegistry oversizedEntities = new(worldScope: 1);
EntityUuid oversizedPlayerId = EntityUuid.Create();
EntityRuntimeHandle oversizedHandle = oversizedEntities.Allocate(oversizedPlayerId);
ArchEntity oversizedPlayer = oversizedWorld.Create();
oversizedWorld.Add(oversizedPlayer, new EntityIdentityComponent(oversizedPlayerId));
oversizedWorld.Add(oversizedPlayer, new LocationComponent(new Vector2(100f, 250f)));
oversizedWorld.Add(oversizedPlayer, new ColliderComponent(new Vector2(16f, 32f)));
oversizedWorld.Add(oversizedPlayer, new LifeComponent(100, 100));
ItemRegistry oversizedDefinitions = new();
oversizedDefinitions.Register(new Item(ItemId.Gel, 99, ItemCategoryMask.Any));
InventoryComponent oversizedInventory = new(
[
  new InventorySlotDefinition(0, InventorySlotKind.General, ItemCategoryMask.Any, 99),
]);
oversizedWorld.Add(oversizedPlayer, oversizedInventory);
oversizedEntities.Register(oversizedHandle, oversizedPlayer);
oversizedSession.BindPlayer(oversizedHandle);
oversizedSession.TryAdvance(CommunicationSessionStage.Active);
InventorySystem oversizedInventories = new(oversizedWorld, oversizedDefinitions);
int oversizedBefore = 150;
InventoryTransferResult oversizedResult = oversizedInventories.TryInsert(
    oversizedPlayer,
    new ItemStack(ItemId.Gel, oversizedBefore));
Check(
    "invariant-oversized-stack-rejected-whole",
    oversizedResult.Failure == InventoryTransferFailure.InvalidItemStack &&
        InventoryQuantity(oversizedInventory) == 0,
    $"failure={oversizedResult.Failure} 背包={InventoryQuantity(oversizedInventory)}（超定义上限整堆拒绝，不得部分入包）");

// INV-3：只读端口不得长出写入口。这是结构性守卫——单一写者规则一旦被破坏，
// 通常是先给某个"查询"端口悄悄加一个 Create/Update/Consume 成员。
string[] queryPortMethods = Array.ConvertAll(
    typeof(IWorldItemQuery).GetMethods(),
    static method => method.Name);
string[] projectionPortMethods = Array.ConvertAll(
    typeof(IWorldItemProjectionSource).GetMethods(),
    static method => method.Name);
Array.Sort(queryPortMethods, StringComparer.Ordinal);
Array.Sort(projectionPortMethods, StringComparer.Ordinal);
bool queryPortIsReadOnly = queryPortMethods.Length == 1 && queryPortMethods[0] == "TryGet";
bool projectionPortOnlyDrains = projectionPortMethods.Length == 1 &&
    projectionPortMethods[0] == "DrainMutations";
Check(
    "invariant-single-writer-world-slot",
    queryPortIsReadOnly && projectionPortOnlyDrains,
    $"查询端口=[{string.Join(',', queryPortMethods)}] 投影端口=[{string.Join(',', projectionPortMethods)}]");

// INV-1 的结构面：世界槽的状态必须把物品身份**整体**放在 ItemStack 里，
// 一旦有人把 type/stack/prefix 平铺回 WorldItemState，载荷就有第二份来源。
// 这里断的是"没有平铺"，不是"成员数量固定"——生命周期计数（P2）合法增加，
// 但它们不得替代或复制 ItemStack 的三元组。
string[] stateMembers = Array.FindAll(
    Array.ConvertAll(
        typeof(WorldItemState).GetProperties(),
        static property => property.Name),
    static name => name != "EqualityContract");
string[] flattenedPayloadNames = ["Type", "Stack", "Prefix", "ItemType", "Quantity", "ItemId"];
string[] offendingMembers = Array.FindAll(
    stateMembers,
    member => Array.IndexOf(flattenedPayloadNames, member) >= 0);
bool stateCarriesSinglePayload = offendingMembers.Length == 0 &&
    Array.IndexOf(stateMembers, "Item") >= 0;
Check(
    "invariant-world-state-carries-no-second-payload",
    stateCarriesSinglePayload,
    $"成员=[{string.Join(',', stateMembers)}]；平铺字段=[{string.Join(',', offendingMembers)}]（必须为空）");

// 载荷只有一个"形状"：状态里那个 Item 属性的类型必须是 ItemStack 本身，
// 而不是某个把四元组拆开的新类型。
Check(
    "invariant-world-state-item-property-is-item-stack",
    typeof(WorldItemState).GetProperty("Item")?.PropertyType == typeof(ItemStack),
    $"Item 属性类型={typeof(WorldItemState).GetProperty("Item")?.PropertyType.Name}");

// INV-5：世界槽是进程内瞬态，不得出现任何存档型入口。
string[] persistenceWords = ["Save", "Load", "Serialize", "Persist", "Export", "Import", "Read", "Write"];
bool worldStoreHasNoPersistenceApi = true;
string offendingMember = "none";
foreach (System.Reflection.MethodInfo method in typeof(WorldItemStore).GetMethods(
    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
{
  foreach (string word in persistenceWords)
  {
    if (method.Name.Contains(word, StringComparison.Ordinal))
    {
      worldStoreHasNoPersistenceApi = false;
      offendingMember = method.Name;
      break;
    }
  }

  if (!worldStoreHasNoPersistenceApi)
  {
    break;
  }
}

Check(
    "invariant-world-items-not-persisted",
    worldStoreHasNoPersistenceApi,
    $"命中成员={offendingMember}（世界物品只存在于运行时，存档段不得引用）");

// --- 13. 世界侧生命周期计数（P2）：noGrabDelay 门控与计数推进 ---
// 对照 Version4 WorldItem.UpdateItem 与 Player.GetItem(WorldItem, …)：
//   · noGrabDelay > 0 时该物品不可被拾取（Player.cs:38653 直接返回原物品）
//   · noGrabDelay 每 tick 递减到 0 为止（WorldItem.cs:603-606）
//   · timeSinceItemSpawned 每 tick 递增，上限 2147483547（WorldItem.cs:599-602）
using ArchWorld delayWorld = ArchWorld.Create();
WorldRuntime delayRuntime = BuildFloorRuntime(worldWidth: 30, worldHeight: 20, floorRow: 18);
_ = delayWorld.Create(new WorldItemRequestQueueComponent());
_ = delayWorld.Create(new InventoryPickupRequestQueueComponent());
SessionEpoch delayEpoch = SessionEpoch.Create(23);
CommunicationSessionComponent delaySession = new(delayEpoch);
_ = delayWorld.Create(
    delaySession,
    new CommunicationInboundMessageBufferComponent(),
    new CommunicationOutboundMessageBufferComponent());
EntityRuntimeRegistry delayEntities = new(worldScope: 1);
EntityUuid delayPlayerId = EntityUuid.Create();
EntityRuntimeHandle delayHandle = delayEntities.Allocate(delayPlayerId);
ArchEntity delayPlayer = delayWorld.Create();
delayWorld.Add(delayPlayer, new EntityIdentityComponent(delayPlayerId));
delayWorld.Add(delayPlayer, new LocationComponent(new Vector2(100f, 250f)));
delayWorld.Add(delayPlayer, new ColliderComponent(new Vector2(16f, 32f)));
delayWorld.Add(delayPlayer, new LifeComponent(100, 100));
ItemRegistry delayDefinitions = new();
delayDefinitions.Register(new Item(ItemId.Gel, 99, ItemCategoryMask.Any));
InventoryComponent delayInventory = new(
[
  new InventorySlotDefinition(0, InventorySlotKind.General, ItemCategoryMask.Any, 99),
]);
delayWorld.Add(delayPlayer, delayInventory);
delayEntities.Register(delayHandle, delayPlayer);
delaySession.BindPlayer(delayHandle);
delaySession.TryAdvance(CommunicationSessionStage.Active);
WorldItemStore delayStore = new();
delayStore.TryCreate(
    ItemIdRules.ToNetworkType(ItemId.Gel),
    5,
    0,
    new Vector2(100f, 272f),
    Vector2.Zero,
    reservedFor: null,
    noGrabDelay: 3,
    shimmered: false,
    out WorldItemState delayedItem);
delayStore.DrainMutations();
Check(
    "no-grab-delay-blocks-pickup",
    delayStore.TryGet(delayedItem.Slot, out WorldItemState? delayedLive) &&
        delayedLive is not null &&
        !WorldItemRules.CanBePickedUp(delayedLive),
    $"noGrabDelay={(delayedLive is null ? -1 : delayedLive.NoGrabDelay)}（> 0 时不可拾取）");

InventorySystem delayInventories = new(delayWorld, delayDefinitions);
WorldItemTickSystem delayTick = new(
    delayWorld,
    delayStore,
    delayRuntime,
    delayRuntime,
    new SessionPlayerSource(delayWorld, delayEntities),
    maximumStackSize: 99);
InventoryPickupRequestSystem delayRequests = new(delayWorld, delayEntities, delayInventories);
WorldItemRequestSystem delayWorldRequests = new(delayWorld, delayStore);
void StepDelayTick(int tick)
{
  EcsServerTickContext delayContext = new(delayWorld, tick, 1f / 60f);
  delayTick.Update(delayContext);
  delayRequests.Update(delayContext);
  delayWorldRequests.Update(delayContext);
}

// 延迟窗口内（3 个 tick）一次都不能入包：这证明门控真的生效，
// 而不只是"第一次没抓到"。
for (int tick = 0; tick < 3; tick++)
{
  StepDelayTick(tick);
}

_ = delayStore.TryGet(delayedItem.Slot, out WorldItemState? duringDelay);
Check(
    "no-grab-delay-blocks-pickup-for-whole-window",
    duringDelay is not null &&
        duringDelay.NoGrabDelay == 0 &&
        InventoryQuantity(delayInventory) == 0 &&
        delayStore.TryGet(delayedItem.Slot, out _),
    $"延迟窗口内 delay={duringDelay?.NoGrabDelay} 背包={InventoryQuantity(delayInventory)} 槽仍在={delayStore.TryGet(delayedItem.Slot, out _)}");

// 延迟归零后的下一 tick 才允许拾取——顺序与 Version4 一致（先判定后递减）。
StepDelayTick(3);
Check(
    "no-grab-delay-expires-then-pickup",
    InventoryQuantity(delayInventory) == 5 &&
        !delayStore.TryGet(delayedItem.Slot, out _),
    $"延迟耗尽后背包={InventoryQuantity(delayInventory)} 槽已退役={!delayStore.TryGet(delayedItem.Slot, out _)}");

// 计数推进：生成即从 0 开始，每 tick +1，延迟每 tick −1。
WorldItemStore counterStore = new();
counterStore.TryCreate(
    ItemIdRules.ToNetworkType(ItemId.Gel),
    1,
    0,
    new Vector2(0f, 0f),
    Vector2.Zero,
    reservedFor: null,
    noGrabDelay: 2,
    shimmered: false,
    out WorldItemState counterItem);
Check(
    "lifecycle-counters-start-at-declared",
    counterItem.NoGrabDelay == 2 && counterItem.TimeSinceItemSpawned == 0,
    $"delay={counterItem.NoGrabDelay} spawned={counterItem.TimeSinceItemSpawned}");
for (int tick = 0; tick < 5; tick++)
{
  counterStore.TryAdvanceLifecycle(counterItem.Slot, out _);
}

_ = counterStore.TryGet(counterItem.Slot, out WorldItemState? counted);
Check(
    "lifecycle-counts-advance-per-tick",
    counted is not null && counted.NoGrabDelay == 0 && counted.TimeSinceItemSpawned == 5,
    $"delay={counted?.NoGrabDelay} spawned={counted?.TimeSinceItemSpawned}（5 tick 后应为 0 / 5）");

// 合并判定必须区分微光产物：Version4 要求 shimmered 相同才合并。
Check(
    "merge-requires-same-shimmer-state",
    WorldItemMergeRules.MergeAmount(
        survivor with { Slot = 1, Shimmered = true },
        survivor,
        99) == 0,
    "微光标记不同不合并");

// --- 14. 新建哨兵语义（INV-6/INV-7） ---
// Version4 客户端丢弃物品时发的包 21 里，槽位填 400（"请新建"哨兵）；
// 服务端会自己调 Item.NewItem 重新分配槽位（MessageBuffer.cs:1485-1492），
// 客户端给的 400 从不被当真槽位用。这里锁住世界宿主的行为：
//   · Create 的 Slot 是哨兵，宿主忽略它并自己分配；
//   · 分配永远取最低空闲槽，不复用仍被占用的槽位。
using ArchWorld sentinelWorld = ArchWorld.Create();
_ = sentinelWorld.Create(new WorldItemRequestQueueComponent());
WorldItemRequestQueueComponent sentinelQueue =
    WorldItemRequestQueue.Require(sentinelWorld);
WorldItemStore sentinelStore = new();
WorldItemRequestSystem sentinelRequests = new(sentinelWorld, sentinelStore);

sentinelQueue.Enqueue(new WorldItemRequest(
    WorldItemRequestKind.Create,
    Slot: 400,
    Actor: actor,
    ItemType: ItemIdRules.ToNetworkType(ItemId.Gel),
    Stack: 3,
    Prefix: 0,
    Position: new Vector2(0f, 0f),
    Velocity: Vector2.Zero));
sentinelRequests.Update(new EcsServerTickContext(sentinelWorld, 0, 1f / 60f));

WorldItemState[] sentinelItems = sentinelStore.SnapshotItems();
Check(
    "create-ignores-sentinel-slot",
    sentinelItems.Length == 1 && sentinelItems[0].Slot != 400,
    $"落点={(sentinelItems.Length == 1 ? sentinelItems[0].Slot : -1)}（哨兵 400 不得成为真实槽位）");

// 分配必须取最低空闲槽，且不复用仍被占用的槽位。
sentinelQueue.Enqueue(new WorldItemRequest(
    WorldItemRequestKind.Create,
    Slot: 400,
    Actor: actor,
    ItemType: ItemIdRules.ToNetworkType(ItemId.Gel),
    Stack: 4,
    Prefix: 0,
    Position: new Vector2(20f, 0f),
    Velocity: Vector2.Zero));
sentinelRequests.Update(new EcsServerTickContext(sentinelWorld, 1, 1f / 60f));

WorldItemState[] sentinelAfterSecond = sentinelStore.SnapshotItems();
bool tookLowestThenNext = sentinelAfterSecond.Length == 2 &&
    sentinelAfterSecond[0].Slot == 0 &&
    sentinelAfterSecond[1].Slot == 1;
Check(
    "create-picks-lowest-free-slot",
    tookLowestThenNext,
    $"槽位=[{string.Join(',', Array.ConvertAll(sentinelAfterSecond, static item => item.Slot))}]（应为 0,1）");

// 退役中间槽后再建：必须回填最低空闲槽 0，而不是追加到 2。
_ = sentinelStore.TryRetire(0, actor, out _);
sentinelStore.DrainMutations();
sentinelQueue.Enqueue(new WorldItemRequest(
    WorldItemRequestKind.Create,
    Slot: 400,
    Actor: actor,
    ItemType: ItemIdRules.ToNetworkType(ItemId.Gel),
    Stack: 5,
    Prefix: 0,
    Position: new Vector2(40f, 0f),
    Velocity: Vector2.Zero));
sentinelRequests.Update(new EcsServerTickContext(sentinelWorld, 2, 1f / 60f));

WorldItemState[] sentinelAfterRefill = sentinelStore.SnapshotItems();
Check(
    "create-never-reuses-occupied-slot",
    sentinelAfterRefill.Length == 2 &&
        sentinelAfterRefill[0].Slot == 0 &&
        sentinelAfterRefill[1].Slot == 1,
    $"槽位=[{string.Join(',', Array.ConvertAll(sentinelAfterRefill, static item => item.Slot))}]（退役 0 后回填 0，1 不动）");

Console.WriteLine();
if (failures == 0)
{
  Console.WriteLine("PASS: world item identity, timing and pickup facts hold.");
  return 0;
}

Console.WriteLine($"FAIL: {failures} check(s) failed.");
return 1;

static WorldRuntime BuildFloorRuntime(int worldWidth, int worldHeight, int floorRow)
{
  WorldRuntime runtime = new(worldWidth, worldHeight);
  for (int x = 0; x < worldWidth; x++)
  {
    for (int y = floorRow; y < worldHeight; y++)
    {
      runtime.TrySetTile(new WorldTileCoordinate(x, y), new WorldTileState(true, 1, 0, 0, 0));
    }
  }

  return runtime;
}
