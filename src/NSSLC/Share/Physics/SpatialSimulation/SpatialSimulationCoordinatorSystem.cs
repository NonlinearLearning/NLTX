using System;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Entities;
using Terraria.Share.Liquid;
using Terraria.Share.Movement;
using Terraria.Share.Relationships;
using MovementVelocityComponent = Terraria.Share.Movement.VelocityComponent;

namespace Terraria.Share.Physics;

/// <summary>
/// Runs the local spatial step for every entity with the required spatial composition.
/// </summary>
/// <remarks>
/// Spatial participation is determined by spatial components and capabilities. Combat state such as
/// life, faction and attributes is intentionally outside this query; those facts are evaluated by
/// the systems that own damage admission and lifecycle policy. Only location, velocity, grounding
/// and liquid contact are written here.
/// </remarks>
public sealed class SpatialSimulationCoordinatorSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly ISpatialWorldQuery _spatialWorld;
  private readonly IEntityRuntimeHandleResolver? _handles;
  private readonly SpatialSimulationSystem _simulation;
  private readonly QueryDescription _query;

  public SpatialSimulationCoordinatorSystem(
      ArchWorld world,
      ISpatialWorldQuery spatialWorld,
      SpatialSimulationSystem? simulation = null,
      IEntityRuntimeHandleResolver? handles = null)
      : base(world ?? throw new ArgumentNullException(nameof(world)))
  {
    ArgumentNullException.ThrowIfNull(spatialWorld);
    _spatialWorld = spatialWorld;
    _handles = handles;
    _simulation = simulation ?? new SpatialSimulationSystem();
    _query = new QueryDescription()
        .WithAll<
            LocationComponent,
            MovementVelocityComponent,
            ColliderComponent,
            GroundingComponent>();
  }

  /// <summary>瓦片接触队列存在时返回它；不存在返回 null，调用方据此跳过接触捕获。</summary>
  private TileContactQueueComponent? TryGetTileContactQueue()
  {
    TileContactQueueComponent? queue = null;
    QueryDescription query = new QueryDescription().WithAll<TileContactQueueComponent>();
    World.Query(in query, (ref TileContactQueueComponent component) => queue = component);
    return queue;
  }

  public override void Update(in EcsServerTickContext context)
  {
    long tick = context.Tick;
    float deltaTime = context.DeltaTime;
    SpatialWorldRevision revision = _spatialWorld.Revision;
    SpatialFrame frame = new SpatialFrame(tick, revision, _spatialWorld);
    // 瓦片接触队列可选：没有它的世界照样跑空间推进，只是没人订阅接触事实。
    TileContactQueueComponent? tileContacts = TryGetTileContactQueue();
    tileContacts?.BeginCapture(tick, revision);
    World.Query(
        in _query,
        (ArchEntity entity,
            ref LocationComponent location,
            ref MovementVelocityComponent velocity,
            ref ColliderComponent collider,
            ref GroundingComponent grounding) =>
        {
          if (!collider.Enabled)
          {
            return;
          }

          if (!World.TryGet(entity, out MovementCapabilityComponent movement) ||
              !World.TryGet(entity, out GravityCapabilityComponent gravityCapability) ||
              !World.TryGet(entity, out GravityStateComponent gravity) ||
              !World.TryGet(entity, out TileCollisionCapabilityComponent tileCollision) ||
              !World.TryGet(entity, out LiquidInteractionCapabilityComponent liquidInteraction) ||
              !World.TryGet(entity, out MovementIntent intent))
          {
            return;
          }

          LiquidContactComponent liquidContact = World.TryGet(
              entity, out LiquidContactComponent existingLiquid)
              ? existingLiquid
              : new LiquidContactComponent(LiquidContactFlags.None, 0);
          SubstepCapabilityComponent substep = World.TryGet(
              entity, out SubstepCapabilityComponent declaredSubsteps)
              ? declaredSubsteps
              : SubstepCapabilityComponent.Single;
          SpatialEntitySnapshot<ArchEntity> snapshot = new SpatialEntitySnapshot<ArchEntity>(
              entity,
              location,
              velocity,
              collider,
              grounding,
              liquidContact,
              substep);
          SpatialSimulationResult<ArchEntity> result = _simulation.Simulate(
              in frame,
              in snapshot,
              in movement,
              in gravityCapability,
              in gravity,
              in tileCollision,
              in liquidInteraction,
              in intent,
              deltaTime);
          if (result.Status != SpatialSimulationStatus.Applied ||
              _spatialWorld.Revision != revision)
          {
            return;
          }

          if (tileContacts is not null &&
              result.Commit.HasTileContact &&
              _handles is not null &&
              _handles.TryGetHandle(entity, out EntityRuntimeHandle contactHandle))
          {
            tileContacts.Add(new TileContactFact(
                contactHandle,
                result.Commit.TileContact,
                result.Commit.IncomingVelocity,
                tick,
                revision,
                result.Commit.Substep));
          }

          SpatialCommit commit = result.Commit;
          location = commit.Location;
          velocity = commit.Velocity;
          grounding = commit.Grounding;
          if (World.Has<LiquidContactComponent>(entity))
          {
            ref LiquidContactComponent currentLiquid = ref World.Get<LiquidContactComponent>(entity);
            currentLiquid = commit.LiquidContact;
          }
        });
  }
}
