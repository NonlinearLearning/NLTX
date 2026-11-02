using System;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Entity;
using Terraria.Liquid;
using Terraria.Movement;
using MovementVelocityComponent = Terraria.Movement.VelocityComponent;

namespace Terraria.Physics;

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
  private readonly SpatialSimulationSystem _simulation;
  private readonly QueryDescription _query;

  public SpatialSimulationCoordinatorSystem(
      ArchWorld world,
      ISpatialWorldQuery spatialWorld,
      SpatialSimulationSystem? simulation = null)
      : base(world ?? throw new ArgumentNullException(nameof(world)))
  {
    ArgumentNullException.ThrowIfNull(spatialWorld);
    _spatialWorld = spatialWorld;
    _simulation = simulation ?? new SpatialSimulationSystem();
    _query = new QueryDescription()
        .WithAll<
            LocationComponent,
            MovementVelocityComponent,
            ColliderComponent,
            GroundingComponent>();
  }

  public override void Update(in EcsServerTickContext context)
  {
    long tick = context.Tick;
    float deltaTime = context.DeltaTime;
    SpatialWorldRevision revision = _spatialWorld.Revision;
    SpatialFrame frame = new SpatialFrame(tick, revision, _spatialWorld);
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
          SpatialEntitySnapshot<ArchEntity> snapshot = new SpatialEntitySnapshot<ArchEntity>(
              entity,
              location,
              velocity,
              collider,
              grounding,
              liquidContact);
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
