using System;
using System.Collections.Generic;
using System.Numerics;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Ai;
using Terraria.Combat;
using Terraria.Communication;
using Terraria.Entity;
using Terraria.Liquid;
using Terraria.Movement;
using Terraria.Npc;
using Terraria.Physics;
using Terraria.Relationships;
using Terraria.World;
using MovementVelocityComponent = Terraria.Movement.VelocityComponent;

// 刷怪系统的行为验证：
//   1. 出生预算公式逐档对照 Version4 Spawner.GetSpawnRate。
//   2. 出生区域/安全区/落点搜索与 Version4 GetSpawnArea / FindSpawnTile 一致。
//   3. 整条链路：会话玩家 → 刷怪 → NPC 实体落地 → Network 槽位分配。
int failures = 0;

void Check(string name, bool ok, string detail)
{
  Console.WriteLine($"{(ok ? "PASS" : "FAIL")} {name} | {detail}");
  if (!ok)
  {
    failures++;
  }
}

// --- 1. 出生预算 ---
NpcSpawnEnvironment surfaceDay = new(
    HardMode: false,
    WorldSurfaceTileY: 80,
    RockLayerTileY: 120,
    UnderworldTileY: 170,
    DayTime: true,
    BloodMoon: false,
    Eclipse: false);
NpcSpawnBudget dayBudget = NpcSpawnBudgetRules.For(surfaceDay, new Vector2(0f, 16f * 50f), 16f);
Check(
    "budget-surface-day",
    dayBudget == new NpcSpawnBudget(600, 5),
    $"spawnRate={dayBudget.SpawnRate} max={dayBudget.MaximumSpawns}（默认 600/5）");

NpcSpawnBudget nightBudget = NpcSpawnBudgetRules.For(
    surfaceDay with { DayTime = false },
    new Vector2(0f, 16f * 50f),
    16f);
Check(
    "budget-surface-night",
    nightBudget == new NpcSpawnBudget(360, 6),
    $"spawnRate={nightBudget.SpawnRate} max={nightBudget.MaximumSpawns}（600×0.6 / 5×1.3）");

NpcSpawnBudget bloodMoonBudget = NpcSpawnBudgetRules.For(
    surfaceDay with { DayTime = false, BloodMoon = true },
    new Vector2(0f, 16f * 50f),
    16f);
Check(
    "budget-blood-moon",
    bloodMoonBudget.SpawnRate == 108 && bloodMoonBudget.MaximumSpawns == 10,
    $"spawnRate={bloodMoonBudget.SpawnRate} max={bloodMoonBudget.MaximumSpawns}（360×0.3 / 6×1.8）");

NpcSpawnBudget undergroundBudget = NpcSpawnBudgetRules.For(
    surfaceDay,
    new Vector2(0f, 16f * 100f),
    16f);
Check(
    "budget-underground",
    undergroundBudget == new NpcSpawnBudget(300, 8),
    $"spawnRate={undergroundBudget.SpawnRate} max={undergroundBudget.MaximumSpawns}（600×0.5 / 5×1.7）");

NpcSpawnBudget cavernBudget = NpcSpawnBudgetRules.For(
    surfaceDay,
    new Vector2(0f, 16f * 140f),
    16f);
Check(
    "budget-cavern",
    cavernBudget == new NpcSpawnBudget(240, 9),
    $"spawnRate={cavernBudget.SpawnRate} max={cavernBudget.MaximumSpawns}（600×0.4 / 5×1.9）");

NpcSpawnBudget underworldBudget = NpcSpawnBudgetRules.For(
    surfaceDay,
    new Vector2(0f, 16f * 180f),
    16f);
Check(
    "budget-underworld",
    underworldBudget == new NpcSpawnBudget(600, 10),
    $"spawnRate={underworldBudget.SpawnRate} max={underworldBudget.MaximumSpawns}（上限 ×2）");

NpcSpawnBudget eclipseBudget = NpcSpawnBudgetRules.For(
    surfaceDay with { Eclipse = true },
    new Vector2(0f, 16f * 50f),
    16f);
Check(
    "budget-eclipse",
    eclipseBudget.SpawnRate == 120 && eclipseBudget.MaximumSpawns == 15,
    $"spawnRate={eclipseBudget.SpawnRate} max={eclipseBudget.MaximumSpawns}（600×0.2 / 5×3 封顶）");

// --- 2. 出生区域与落点 ---
NpcSpawnArea area = NpcSpawnAreaRules.GetSpawnArea(
    new Vector2(100f * 16f, 100f * 16f),
    tileSize: 16f,
    worldWidth: 400,
    worldHeight: 300);
Check(
    "spawn-area-extents",
    area.Spawn.Width == 168 && area.Spawn.Height == 94 &&
        area.Safe.Width == 124 && area.Safe.Height == 70,
    $"spawn={area.Spawn.Width}x{area.Spawn.Height} safe={area.Safe.Width}x{area.Safe.Height}");
Check(
    "spawn-area-clamped",
    area.Spawn.Left == 16 && area.Spawn.Top == 53,
    $"left={area.Spawn.Left} top={area.Spawn.Top}（100-84，100-47）");

NpcSpawnArea cornerArea = NpcSpawnAreaRules.GetSpawnArea(
    new Vector2(2f * 16f, 2f * 16f),
    tileSize: 16f,
    worldWidth: 400,
    worldHeight: 300);
Check(
    "spawn-area-clamps-to-world",
    cornerArea.Spawn.Left == 0 && cornerArea.Spawn.Top == 0 && cornerArea.Spawn.IsValid,
    $"left={cornerArea.Spawn.Left} top={cornerArea.Spawn.Top}");

using ArchWorld eligibilityWorld = ArchWorld.Create();
WorldRuntime eligibilityRuntime = BuildFloorRuntime(worldWidth: 400, worldHeight: 300, floorRow: 200);
NpcSpawnRandom random = new(seed: 20261106UL);
bool foundSpawnTile = NpcSpawnAreaRules.TryFindSpawnTile(
    area with
    {
      // 搜索区必须把地面行包含在底部之内，否则参考实现的扫描会在边界处放弃。
      Spawn = new WorldTileArea(150, 60, 120, 190),
      Safe = new WorldTileArea(150, 60, 40, 60),
    },
    eligibilityRuntime,
    ref random,
    out WorldTileCoordinate spawnTile);
Check(
    "find-spawn-tile-lands-on-floor",
    foundSpawnTile && spawnTile.Y == 200 && spawnTile.X >= 150 && spawnTile.X < 270,
    $"found={foundSpawnTile} tile={spawnTile.X},{spawnTile.Y}（地面行 200，列在搜索区内）");
Check(
    "spawn-tile-has-body-space",
    foundSpawnTile && NpcSpawnAreaRules.HasSpawnSpace(eligibilityRuntime, spawnTile.X, spawnTile.Y),
    "2x3 空间可用");

Check(
    "spawn-tile-has-no-body-space",
    !NpcSpawnAreaRules.HasSpawnSpace(
        SetTile(eligibilityRuntime, 12, 199, new WorldTileState(true, 1, 0, 0, 0)),
        12,
        200),
    "落地格上方被实心方块占住时不可出生");
Check(
    "spawn-tile-rejects-lava",
    !NpcSpawnAreaRules.CanSpawnInTile(
        SetTile(eligibilityRuntime, 300, 199, new WorldTileState(false, 0, 0, 0, 0, LiquidContactFlags.Lava, 255)),
        300,
        199),
    "岩浆格不可出生");
Check(
    "spawn-tile-accepts-water",
    NpcSpawnAreaRules.CanSpawnInTile(
        SetTile(eligibilityRuntime, 301, 199, new WorldTileState(false, 0, 0, 0, 0, LiquidContactFlags.Water, 255)),
        301,
        199),
    "水格可出生");

List<SessionPlayer> players =
[
  new SessionPlayer(SessionEpoch.Create(1), default, new Vector2(100f * 16f, 100f * 16f), new Vector2(16f, 32f), true),
];
Check(
    "on-screen-check-rejects-visible",
    NpcSpawnAreaRules.IsVisibleToAnyPlayer(100, 100, 16f, players, area.SafeHalfExtentX, area.SafeHalfExtentY),
    "玩家附近的落点视为可见");
Check(
    "on-screen-check-accepts-hidden",
    !NpcSpawnAreaRules.IsVisibleToAnyPlayer(200, 100, 16f, players, area.SafeHalfExtentX, area.SafeHalfExtentY),
    "屏幕外 100 格的落点不可见");

// --- 3. 端到端：刷怪 + 槽位分配 ---
using ArchWorld spawnWorld = ArchWorld.Create();
WorldRuntime spawnRuntime = BuildFloorRuntime(worldWidth: 400, worldHeight: 300, floorRow: 200);
_ = spawnWorld.Create(new NpcSpawnFactQueueComponent());
SessionEpoch spawnEpoch = SessionEpoch.Create(9);
CommunicationSessionComponent session = new(spawnEpoch);
_ = spawnWorld.Create(
    session,
    new CommunicationInboundMessageBufferComponent(),
    new CommunicationOutboundMessageBufferComponent());
EntityRuntimeRegistry spawnEntities = new(worldScope: 1);
EntityUuid playerId = EntityUuid.Create();
EntityRuntimeHandle playerHandle = spawnEntities.Allocate(playerId);
ArchEntity player = spawnWorld.Create();
spawnWorld.Add(player, new EntityIdentityComponent(playerId));
spawnWorld.Add(player, new LocationComponent(new Vector2(100f * 16f, 185f * 16f)));
spawnWorld.Add(player, new ColliderComponent(new Vector2(16f, 32f)));
spawnWorld.Add(player, new LifeComponent(100, 100));
spawnEntities.Register(playerHandle, player);
session.BindPlayer(playerHandle);
session.TryAdvance(CommunicationSessionStage.Active);

EntityDefinitionRegistry definitions = new();
definitions.Register(
    new EntityDefinitionBuilder(new EntityDefinitionId("npc.spawn-test"))
        .Add(() => new EntityIdentityComponent(EntityUuid.Create()))
        .Add(() => new EntityFactionComponent(Faction.Hostile))
        .Add(() => new LifeComponent(50, 50))
        .Add(() => new LocationComponent(Vector2.Zero))
        .Add(() => MovementVelocityComponent.Stationary)
        .Add(() => new ColliderComponent(new Vector2(24f, 18f)))
        .Add(() => new NpcBehaviorStateComponent())
        .WithAi<SlimeAiBehavior>()
        .Build());
EntityDefinitionSystem<ArchEntity> spawner = new(
    definitions,
    new ArchEntityDefinitionHost(spawnWorld, spawnEntities));
using NpcAiRuntime ai = new(spawnWorld);
NpcSpawnTable table = new(
[
  new NpcSpawnCandidate(
      new EntityDefinitionId("npc.spawn-test"),
      NetworkType: 1,
      AiComponent.For<SlimeAiBehavior>(),
      Weight: 1),
]);
NpcSpawnEnvironment spawnEnvironment = new(
    HardMode: false,
    WorldSurfaceTileY: 80,
    RockLayerTileY: 120,
    UnderworldTileY: 170,
    DayTime: true,
    BloodMoon: false,
    Eclipse: false);
NpcSpawnSystem spawnSystem = new(
    spawnWorld,
    spawnRuntime,
    spawnEntities,
    spawner,
    ai,
    table,
    spawnEnvironment,
    worldSeed: 424242);
for (int tick = 0; tick < 4000; tick++)
{
  spawnSystem.Update(new EcsServerTickContext(spawnWorld, tick, 1f / 60f));
}

List<ArchEntity> spawned = [];
spawnWorld.Query(
    in new Arch.Core.QueryDescription().WithAll<NpcBehaviorStateComponent, ColliderComponent>(),
    (ArchEntity entity) => spawned.Add(entity));
int spawnedCount = spawned.Count;
Check(
    "spawn-system-creates-npcs",
    spawnedCount > 0,
    $"4000 tick 内产生 {spawnedCount} 个 NPC");

bool allGrounded = true;
foreach (ArchEntity npc in spawned)
{
  Vector2 position = spawnWorld.Get<LocationComponent>(npc).Coordinates;
  Vector2 size = spawnWorld.Get<ColliderComponent>(npc).Size;
  allGrounded &= MathF.Abs(position.Y + size.Y - 200f * 16f) < 1f;
}

Check(
    "spawned-npcs-stand-on-ground",
    allGrounded,
    "每个 NPC 的底边都贴在地面行顶边");

NpcSpawnFactQueueComponent facts = NpcSpawnFactQueue.Require(spawnWorld);
Check(
    "spawn-system-raises-network-facts",
    facts.Count == spawnedCount,
    $"待绑定事实 {facts.Count} 条，对应 {spawnedCount} 个 NPC");

Terraria.Network.NpcNetworkIdentitySystem identity = new(spawnWorld);
identity.Update(new EcsServerTickContext(spawnWorld, 4000, 1f / 60f));
HashSet<byte> slots = [];
bool allBound = true;
foreach (ArchEntity npc in spawned)
{
  bool hasIdentity = spawnWorld.Has<Terraria.Network.NpcNetworkIdentityComponent>(npc);
  allBound &= hasIdentity;
  if (hasIdentity)
  {
    slots.Add(spawnWorld.Get<Terraria.Network.NpcNetworkIdentityComponent>(npc).Slot);
  }
}

Check(
    "network-identity-assigns-unique-slots",
    allBound && slots.Count == spawnedCount,
    $"bound={allBound} uniqueSlots={slots.Count}/{spawnedCount}");

Console.WriteLine();
if (failures == 0)
{
  Console.WriteLine("PASS: spawn budget, tile search and NPC creation match the reference rules.");
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

static WorldRuntime SetTile(
    WorldRuntime runtime,
    int x,
    int y,
    WorldTileState state)
{
  runtime.TrySetTile(new WorldTileCoordinate(x, y), state);
  return runtime;
}
