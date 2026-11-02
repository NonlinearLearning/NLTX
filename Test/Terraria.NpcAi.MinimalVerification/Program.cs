using System.Buffers.Binary;
using System.Numerics;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Ai;
using Terraria.Combat;
using Terraria.Communication;
using Terraria.Entity;
using Terraria.Liquid;
using Terraria.Movement;
using Terraria.Npc;
using Terraria.Network;
using Terraria.Physics;
using Terraria.Player;
using Terraria.Relationships;
using Terraria.World;
using MovementVelocityComponent = Terraria.Movement.VelocityComponent;

using ArchWorld world = ArchWorld.Create();
EntityRuntimeRegistry entities = new(worldScope: 1);
ArchEntity player = world.Create(
    new EntityFactionComponent(Faction.Friendly),
    new LifeComponent(100, 100),
    new LocationComponent(new Vector2(300f, 100f)),
    new PlayerInventoryComponent());
ArchEntity guide = CreateNpc(world, entities, new Vector2(20f, 100f), isSlime: false);
ArchEntity slime = CreateNpc(world, entities, new Vector2(100f, 100f), isSlime: true);

using NpcAiRuntime ai = new(world);
ai.AttachGuide(guide);
ai.AttachSlime(slime);
CombatAttackDefinitionCatalog attackDefinitions = new();
attackDefinitions.Seal();
// 攻击意图队列现在挂在世界级实体上，验证程序也必须建出这一实体。
_ = world.Create(new CombatAttackIntentQueueComponent());
_ = world.Create(new MovementIntentRequestQueueComponent());
NpcAiIntentAdapter adapter = new(
    world,
    ai.Entities,
    ai.Snapshots,
    entities,
    attackDefinitions);
adapter.Initialize();
// AI 现在只提交移动意图，由 Movement 域的系统安装到组件上。
MovementIntentRequestSystem movementIntents = new(world, entities);

WorldRuntime spatialWorld = new(64, 64);
WorldInteractionResult load = spatialWorld.Load(
    Enumerable.Repeat(WorldTileState.Empty, 64 * 64).ToArray());
if (!load.Succeeded)
{
  throw new InvalidOperationException(load.Failure);
}

SpatialSimulationCoordinatorSystem spatial = new(world, spatialWorld);
adapter.Update(new EcsServerTickContext(world, 0, 1f / 60f));
movementIntents.Update(new EcsServerTickContext(world, 0, 1f / 60f));
ref MovementIntent initialSlimeIntent = ref world.Get<MovementIntent>(slime);
Require(initialSlimeIntent.Source == MovementIntentSource.Ai, "Slime AI intent was not adapted.");
Require(initialSlimeIntent.DesiredVelocity.X > 0f, "Slime did not target the player to its right.");
Require(world.Get<NpcBehaviorStateComponent>(slime).Phase == NpcBehaviorPhase.Chase,
    "Slime did not enter Chase state.");
Require(world.Get<NpcBehaviorStateComponent>(guide).Phase == NpcBehaviorPhase.Idle,
    "Guide did not start in Idle state.");

float originalX = world.Get<LocationComponent>(slime).Coordinates.X;
spatial.Update(new EcsServerTickContext(world, 0, 1f / 60f));
Require(world.Get<LocationComponent>(slime).Coordinates.X > originalX,
    "Spatial simulation did not commit the Slime's movement.");

bool observedJump = false;
bool observedJumpMovement = false;
for (long tick = 1; tick <= 245; tick++)
{
  ref GroundingComponent grounding = ref world.Get<GroundingComponent>(slime);
  grounding = new GroundingComponent(true, false, Vector2.UnitY);
  ref GroundingComponent guideGrounding = ref world.Get<GroundingComponent>(guide);
  guideGrounding = new GroundingComponent(true, false, Vector2.UnitY);
  adapter.Update(new EcsServerTickContext(world, tick, 1f / 60f));
movementIntents.Update(new EcsServerTickContext(world, tick, 1f / 60f));
  if (world.Get<MovementIntent>(slime).JumpRequested)
  {
    observedJump = true;
    float beforeJumpY = world.Get<LocationComponent>(slime).Coordinates.Y;
    spatial.Update(new EcsServerTickContext(world, tick, 1f / 60f));
    observedJumpMovement |= world.Get<LocationComponent>(slime).Coordinates.Y < beforeJumpY;
  }
}

Require(observedJump, "Slime did not request a grounded jump.");
Require(observedJumpMovement, "Spatial simulation did not apply the Slime jump request.");
Require(world.Get<NpcBehaviorStateComponent>(guide).Phase == NpcBehaviorPhase.Wander,
    "Guide did not transition from Idle to Wander.");
Require(world.Get<MovementIntent>(guide).DesiredVelocity.X != 0f,
    "Guide did not produce a walking movement intent.");
for (long tick = 246; tick <= 500; tick++)
{
  ref GroundingComponent guideGrounding = ref world.Get<GroundingComponent>(guide);
  guideGrounding = new GroundingComponent(true, false, Vector2.UnitY);
  adapter.Update(new EcsServerTickContext(world, tick, 1f / 60f));
movementIntents.Update(new EcsServerTickContext(world, tick, 1f / 60f));
}

Require(world.Get<NpcBehaviorStateComponent>(guide).Phase == NpcBehaviorPhase.Idle,
    "Guide did not return to Idle state.");
Require(world.Get<MovementIntent>(guide).DesiredVelocity == Vector2.Zero,
    "Guide retained walking velocity after entering Idle.");
Require(ai.Entities.BindingCount == 2, "Guide and Slime AI bindings were not retained.");
adapter.Dispose();
VerifyNpcNetworkProjection();
string sharedApiSummary = Terraria.Verification.SharedApiUsage.Verify();
Console.WriteLine(sharedApiSummary);
Console.WriteLine(
    "PASS: Guide idle/wander/stop, Slime targeting/jump, AI intent, spatial movement and NPC wire projection.");

static ArchEntity CreateNpc(
    ArchWorld world,
    EntityRuntimeRegistry entities,
    Vector2 position,
    bool isSlime)
{
  EntityUuid id = EntityUuid.Create();
  EntityRuntimeHandle handle = entities.Allocate(id);
  AiComponent ai = isSlime ? AiComponent.For<SlimeAiBehavior>() : AiComponent.For<GuideAiBehavior>();
  ArchEntity entity = world.Create(
      new EntityIdentityComponent(id),
      new EntityFactionComponent(isSlime ? Faction.Hostile : Faction.Friendly),
      new LifeComponent(isSlime ? 50 : 250, isSlime ? 50 : 250),
      new LocationComponent(position),
      MovementVelocityComponent.Stationary,
      new ColliderComponent(isSlime ? new Vector2(24f, 18f) : new Vector2(18f, 40f)),
      GroundingComponent.Airborne,
      new MovementCapabilityComponent(true, isSlime ? 2.5f : 1f, 12f,
          canJump: isSlime, jumpSpeed: isSlime ? 6f : 0f),
      new GravityCapabilityComponent(true, false),
      new GravityStateComponent(0.4f, 10f, GravityDirection.Normal, false, false),
      new TileCollisionCapabilityComponent(true),
      new LiquidInteractionCapabilityComponent(
          true,
          LiquidContactFlags.Water | LiquidContactFlags.Honey |
              LiquidContactFlags.Lava | LiquidContactFlags.Shimmer),
      MovementIntent.Stop(0),
      new NpcBehaviorStateComponent(),
      ai,
      new AiBehaviorSelectionComponent(
          isSlime ? "Slime" : "Guide",
          isSlime ? "slime.default" : "guide.default"));
  entities.Register(handle, entity);
  return entity;
}

static void Require(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

static void VerifyNpcNetworkProjection()
{
  using ArchWorld world = ArchWorld.Create();
  SessionEpoch epoch = SessionEpoch.Create(1);
  CommunicationSessionComponent session = new(epoch);
  session.TryAdvance(CommunicationSessionStage.Active);
  ArchEntity sessionEntity = world.Create(
      session,
      new CommunicationOutboundMessageBufferComponent());
  Vector2 position = new(640f, 320f);
  world.Create(
      new NpcNetworkIdentityComponent(slot: 3, generation: 1, netId: 22),
      new LifeComponent(250, 250),
      new LocationComponent(position),
      MovementVelocityComponent.Stationary,
      new ColliderComponent(new Vector2(18f, 40f)),
      new NpcBehaviorStateComponent(
          phase: NpcBehaviorPhase.Wander,
          stateSlot0: -1f,
          stateSlot1: 2f),
      GroundingComponent.Airborne);

  NpcNetworkProjectionSystem projection = new(world);
  projection.Update(new EcsServerTickContext(world, 0, 1f / 60f));

  CommunicationOutboundMessageBufferComponent outbound =
      world.Get<CommunicationOutboundMessageBufferComponent>(sessionEntity);
  Require(outbound.TryDequeue(out OutboundPacketIntent? intent) && intent is not null,
      "NPC network projection did not enqueue a packet.");
  ReadOnlySpan<byte> payload = intent!.Payload.Span;
  Require(payload.Length == 27, "NPC packet has an unexpected payload length.");
  Require(BinaryPrimitives.ReadSingleLittleEndian(payload[2..6]) == position.X,
      "NPC packet changed the ECS x coordinate.");
  Require(BinaryPrimitives.ReadSingleLittleEndian(payload[6..10]) == position.Y,
      "NPC packet changed the ECS y coordinate.");
  byte flags1 = payload[20];
  Require((flags1 & 0x3c) == 0, "NPC packet leaked ECS behavior state into native AI slots.");
  Require((payload[21] & 0x08) != 0, "NPC packet did not mark the initial slot snapshot.");
  Require((payload[21] & 0x10) == 0,
      "NPC packet incorrectly marked the native shimmer flag.");
  Require((payload[21] & 0x20) != 0,
      "NPC packet did not mark the client-side ECS-authoritative simulation.");
}
