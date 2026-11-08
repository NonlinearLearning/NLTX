using System;
using System.Collections.Generic;
using System.Numerics;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Combat;
using Terraria.Communication;
using Terraria.Entity;
using Terraria.Inventory;
using Terraria.Item;
using Terraria.Liquid;
using Terraria.Movement;
using Terraria.Physics;
using Terraria.Relationships;
using Terraria.World;

// 世界掉落物的物品身份改由共享 ItemStack 承载（见审计 §32 与 §34）。
// 本验证固定三件事：
//   1. 权威状态只接受 ItemId 里的已知类型；未知类型在入口被拒，不会先存下再投影。
//   2. 身份、数量、前缀与协议 itemType 之间双向无损（换模型不改变线上字节）。
//   3. 清除槽位用空 ItemStack 表达，与原来的 Stack = 0 语义等价。
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
    fallEntities,
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
ItemDefinitionRegistry pickupDefinitions = new();
pickupDefinitions.Register(new ItemDefinition(ItemId.Torch, 99, ItemCategoryMask.Any));
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
    pickupEntities,
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
    voidEntities,
    maximumStackSize: 99);
for (int tick = 0; tick < 5; tick++)
{
  voidTick.Update(new EcsServerTickContext(voidWorld, tick, 1f / 60f));
}

Check(
    "unsupported-item-leaves-world",
    !voidStore.TryGet(voidItem.Slot, out _),
    "世界底部无支撑时退役（Version4 CheckInWorld）");

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
