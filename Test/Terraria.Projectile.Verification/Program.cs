using System.Numerics;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Ai;
using Terraria.Attributes;
using Terraria.Combat;
using Terraria.Entity;
using Terraria.Liquid;
using Terraria.Movement;
using Terraria.Physics;
using Terraria.Projectile;
using Terraria.Relationships;
using VelocityComponent = Terraria.Movement.VelocityComponent;

// 验证 ProjectileLifecycleSystem.TrySpawn 产出的投射物能被共享空间管线驱动。
//
// 背景：此前 TrySpawn 创建的实体缺少 GroundingComponent 等六项组件，导致
// SpatialSimulationCoordinatorSystem 的查询一个都匹配不到——投射物实体存在但从不移动。
// 本验证固定两件事：
//   1. 生成出的实体具备协调系统要求的全部十项组件。
//   2. 决定"匀速直线"语义的三个取值正确：加速度为 0（否则会被 MovementIntent.Stop 的
//      零目标速度逐帧减速到静止）、Flight 模式（跳过重力积分）、速度不钳制。
int failures = 0;

void Check(string name, bool ok, string detail)
{
  Console.WriteLine($"{(ok ? "PASS" : "FAIL")} {name} | {detail}");
  if (!ok)
  {
    failures++;
  }
}

using ArchWorld world = ArchWorld.Create();
ProjectileRuntimeRegistry registry = new(worldScope: 1);
ProjectileDefinitionCatalog catalog = new();
ProjectileDefinitionId definitionId = new("probe.arrow");
catalog.Register(new ProjectileDefinition(
    definitionId,
    new AiBehaviorSelectionComponent("Straight", "arrow.default"),
    new ProjectileKinematicsStateComponent(extraUpdates: 0),
    new ProjectileLifetimeStateComponent(remainingTicks: 120),
    new AttributeSetComponent(),
    new ProjectileCollisionPolicyComponent(
        CollideWithTiles: true,
        CollideWithEntities: true,
        IgnoreOwner: true,
        IgnoreSameFaction: false),
    new EntityCollisionCapabilityComponent(Enabled: true, Layer: 1, Mask: uint.MaxValue),
    new CombatAttackDefinitionId("probe.attack")));
catalog.Seal();

ProjectileLifecycleSystem lifecycle = new(world, catalog, registry);

EntityRuntimeHandle ownerHandle = registry.Allocate(EntityUuid.Create());
ArchEntity owner = world.Create();
registry.RegisterOwner(ownerHandle, owner);

ProjectileSpawnRequest request = new(
    definitionId,
    ownerHandle,
    new ProjectileSourceMetadataComponent("probe", "source-1"),
    position: new Vector2(100f, 200f),
    velocity: new Vector2(6f, -3f),
    collider: new ColliderComponent(new Vector2(8f, 8f)),
    size: new EntitySizeComponent(8, 8),
    faction: Faction.Hostile);

ProjectileSpawnResult result = lifecycle.TrySpawn(request);
bool spawned = result.Status == ProjectileLifecycleStatus.Applied &&
    result.Entity != default &&
    world.IsAlive(result.Entity);
Check(
    "spawn-applied",
    spawned,
    $"status={result.Status} alive={(result.Entity != default && world.IsAlive(result.Entity))}");

if (!spawned)
{
  Console.WriteLine();
  Console.WriteLine($"FAIL: spawn rejected, {failures} check(s) failed.");
  return 1;
}

ArchEntity projectile = result.Entity;

// 协调系统的查询要求（WithAll）
Check("has-location", world.Has<LocationComponent>(projectile), "LocationComponent");
Check("has-velocity", world.Has<VelocityComponent>(projectile), "VelocityComponent");
Check("has-collider", world.Has<ColliderComponent>(projectile), "ColliderComponent");
Check("has-grounding", world.Has<GroundingComponent>(projectile), "GroundingComponent");

// 协调系统回调里的 TryGet 要求
Check(
    "has-movement-capability",
    world.Has<MovementCapabilityComponent>(projectile),
    "MovementCapabilityComponent");
Check(
    "has-gravity-capability",
    world.Has<GravityCapabilityComponent>(projectile),
    "GravityCapabilityComponent");
Check("has-gravity-state", world.Has<GravityStateComponent>(projectile), "GravityStateComponent");
Check(
    "has-tile-collision",
    world.Has<TileCollisionCapabilityComponent>(projectile),
    "TileCollisionCapabilityComponent");
Check(
    "has-liquid-interaction",
    world.Has<LiquidInteractionCapabilityComponent>(projectile),
    "LiquidInteractionCapabilityComponent");
Check("has-movement-intent", world.Has<MovementIntent>(projectile), "MovementIntent");

// 匀速直线运动的三个决定性取值
MovementCapabilityComponent movement = world.Get<MovementCapabilityComponent>(projectile);
Check(
    "zero-acceleration",
    movement.Acceleration == 0f,
    $"acceleration={movement.Acceleration}");
Check(
    "no-speed-clamp",
    movement.MaximumSpeed == 0f,
    $"maximumSpeed={movement.MaximumSpeed}");
Check(
    "flight-mode-skips-gravity",
    movement.Mode == MovementMode.Flight,
    $"mode={movement.Mode}");
Check(
    "cannot-jump",
    !movement.CanJump,
    $"canJump={movement.CanJump}");

// MovementIntent.Stop 的 HasDesiredVelocity 为 true，目标速度为零——这正是必须把
// acceleration 保持为 0 的原因；这里同时固定这个前提，避免有人"顺手"改掉 Stop 的语义。
MovementIntent intent = world.Get<MovementIntent>(projectile);
Check(
    "intent-is-stop-with-zero-desired",
    intent.HasDesiredVelocity && intent.DesiredVelocity == Vector2.Zero,
    $"hasDesired={intent.HasDesiredVelocity} desired=({intent.DesiredVelocity.X},{intent.DesiredVelocity.Y})");

// 重力能力关闭，与 Flight 模式互为双保险
GravityCapabilityComponent gravityCapability =
    world.Get<GravityCapabilityComponent>(projectile);
Check(
    "gravity-capability-disabled",
    !gravityCapability.Enabled,
    $"enabled={gravityCapability.Enabled}");

// 出生速度必须原样落在实体上，否则匀速直线就无从谈起
VelocityComponent velocity = world.Get<VelocityComponent>(projectile);
Check(
    "spawn-velocity-preserved",
    velocity.LinearVelocity == new Vector2(6f, -3f),
    $"velocity=({velocity.LinearVelocity.X},{velocity.LinearVelocity.Y})");

// ---- 生成接缝：请求入队 -> 系统排空 -> 实体生成 ----
ArchEntity queueEntity = world.Create(new ProjectileSpawnQueueComponent());
ProjectileSpawnQueueComponent spawnQueue = world.Get<ProjectileSpawnQueueComponent>(queueEntity);
ProjectileSpawnRequest queuedRequest = new(
    definitionId,
    ownerHandle,
    new ProjectileSourceMetadataComponent("probe", "queued-1"),
    position: new Vector2(10f, 20f),
    velocity: new Vector2(-2f, 5f),
    collider: new ColliderComponent(new Vector2(8f, 8f)),
    size: new EntitySizeComponent(8, 8),
    faction: Faction.Hostile);
spawnQueue.Enqueue(queuedRequest, tick: 7);
Check("queue-accepts", spawnQueue.Count == 1, $"count={spawnQueue.Count}");

ProjectileSpawnRequestSystem spawnSystem = new(world, lifecycle);
spawnSystem.Update(new EcsServerTickContext(world, 7, 1f / 60f));
Check(
    "queue-spawned-one",
    spawnSystem.Results.Count == 1 &&
        spawnSystem.Results[0].Status == ProjectileLifecycleStatus.Applied,
    $"results={spawnSystem.Results.Count} status={(spawnSystem.Results.Count == 1 ? spawnSystem.Results[0].Status.ToString() : "n/a")}");
Check("queue-drained", spawnQueue.Count == 0, $"count={spawnQueue.Count}");

// 接缝产出的实体同样必须是可模拟的，否则"接上生产者"只是换了个地方不生效
bool queuedSimulatable = spawnSystem.Results.Count == 1 &&
    world.IsAlive(spawnSystem.Results[0].Entity) &&
    world.Has<GroundingComponent>(spawnSystem.Results[0].Entity) &&
    world.Has<MovementCapabilityComponent>(spawnSystem.Results[0].Entity);
Check("queued-entity-simulatable", queuedSimulatable, "Grounding + MovementCapability");

// ---- 真实位移验证：共享协调系统确实会搬运投射物 ----
// 前面的断言只证明"组件齐备"，这里证明"齐备之后真的会动"，同时用实际速度反证
// acceleration=0 那个决定——若加速度非零，速度会被拉向 MovementIntent.Stop 的零目标。
ProbeSpatialWorld probeWorld = new();
SpatialSimulationCoordinatorSystem coordinator = new(world, probeWorld);
// 生产顺序：先刷新意图，再由协调系统模拟。缺了这一步，投射物的意图 tick 会停在生成时的
// 0，被 intent.Tick != frame.Tick 校验永久拒绝——这正是本条验证要防住的回归。
ProjectileMovementIntentSystem intentSystem = new(world);
intentSystem.Update(new EcsServerTickContext(world, 8, 0.5f));
Check(
    "intent-refreshed-to-current-tick",
    world.Get<MovementIntent>(projectile).Tick == 8,
    $"intentTick={world.Get<MovementIntent>(projectile).Tick}");

Vector2 beforePosition = world.Get<LocationComponent>(projectile).Coordinates;
coordinator.Update(new EcsServerTickContext(world, 8, 0.5f));
Vector2 afterPosition = world.Get<LocationComponent>(projectile).Coordinates;
Vector2 afterVelocity = world.Get<VelocityComponent>(projectile).LinearVelocity;

Check(
    "coordinator-moved-projectile",
    probeWorld.MoveCount > 0 && afterPosition != beforePosition,
    $"moveCount={probeWorld.MoveCount} before=({beforePosition.X},{beforePosition.Y}) after=({afterPosition.X},{afterPosition.Y})");
Vector2 expectedPosition = beforePosition + new Vector2(6f, -3f) * 0.5f;
Check(
    "movement-is-velocity-times-dt",
    afterPosition == expectedPosition,
    $"expected=({expectedPosition.X},{expectedPosition.Y}) actual=({afterPosition.X},{afterPosition.Y})");
Check(
    "velocity-unchanged-no-deceleration",
    afterVelocity == new Vector2(6f, -3f),
    $"velocity=({afterVelocity.X},{afterVelocity.Y})");

Console.WriteLine();
Console.WriteLine(
    failures == 0
        ? "PASS: projectile spawn produces a spatial-simulatable entity."
        : $"FAIL: {failures} check(s) failed.");
return failures == 0 ? 0 : 1;

/// <summary>自由移动的 <see cref="ISpatialWorldQuery"/> 桩：不查询瓦片，位移直接等于位移量。</summary>
internal sealed class ProbeSpatialWorld : ISpatialWorldQuery
{
  public SpatialWorldRevision Revision => SpatialWorldRevision.Initial;

  public int MoveCount { get; private set; }

  public SpatialTileMotionResult Move(
      Vector2 position,
      Vector2 velocity,
      Vector2 colliderSize,
      TileCollisionCapabilityComponent capability,
      GravityDirection gravityDirection)
  {
    MoveCount++;
    return new SpatialTileMotionResult(position + velocity, velocity, false, false);
  }

  public SpatialLiquidContact GetLiquidContact(
      Vector2 position,
      Vector2 colliderSize,
      LiquidInteractionCapabilityComponent capability)
  {
    return new SpatialLiquidContact(LiquidContactFlags.None, 0);
  }
}
