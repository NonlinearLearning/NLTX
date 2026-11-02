using System;
using System.Numerics;
using Terraria.Movement;
using Terraria.Physics;

namespace Terraria.Projectile;

/// <summary>Computes projectile movement from explicitly supplied component values.</summary>
public sealed class ProjectileSimulationSystem
{
  public ProjectileSimulationResult Simulate(
      in SpatialFrame frame,
      in Terraria.Entity.LocationComponent location,
      in VelocityComponent velocity,
      in ColliderComponent collider,
      in ProjectileKinematicsStateComponent kinematics,
      in ProjectileLifetimeStateComponent lifetime,
      in ProjectileCollisionPolicyComponent collisionPolicy,
      in ProjectileDispositionStateComponent disposition,
      in ProjectileAiIntent intent,
      in TileCollisionCapabilityComponent tileCollision,
      in GravityCapabilityComponent gravityCapability,
      in GravityStateComponent gravity,
      float deltaTime)
  {
    if (!frame.Revision.IsValid)
    {
      return Reject(frame.Revision, location, velocity, lifetime, kinematics,
          ProjectileSimulationStatus.NotReady, "The spatial frame revision is invalid.");
    }

    if (!float.IsFinite(deltaTime) || deltaTime <= 0f)
    {
      return Reject(frame.Revision, location, velocity, lifetime, kinematics,
          ProjectileSimulationStatus.NotReady, "The projectile delta time must be finite and positive.");
    }

    if (!disposition.IsActive)
    {
      return Reject(frame.Revision, location, velocity, lifetime, kinematics,
          ProjectileSimulationStatus.Conflict, "Only an active projectile can be simulated.");
    }

    if (lifetime.IsExpired)
    {
      return ProjectileSimulationResult.Create(
          ProjectileSimulationStatus.Expired, frame.Revision,
          CreateMotion(location, velocity, false, false, 0),
          kinematics.BeginTick(), false);
    }

    if (intent.Kind == ProjectileAiIntentKind.RequestTermination)
    {
      return ProjectileSimulationResult.Create(
          ProjectileSimulationStatus.TerminationRequested, frame.Revision,
          CreateMotion(location, velocity, false, false, 0),
          kinematics.BeginTick(), false);
    }

    Vector2 nextVelocity = intent.Kind == ProjectileAiIntentKind.AdjustVelocity
        ? intent.DesiredVelocity
        : velocity.LinearVelocity;
    if (!Terraria.Physics.SimulationVector.IsFinite(nextVelocity))
    {
      return Reject(frame.Revision, location, new VelocityComponent(nextVelocity), lifetime,
          kinematics, ProjectileSimulationStatus.Rejected,
          "The projectile intent contains a non-finite velocity.");
    }

    GravityDirection gravityDirection = gravity.Direction;
    if (gravityDirection == GravityDirection.Unspecified)
    {
      gravityDirection = GravityDirection.Normal;
    }

    if (gravityCapability.Enabled)
    {
      if (gravity.Direction is not (GravityDirection.Normal or GravityDirection.Reversed))
      {
        return Reject(frame.Revision, location, new VelocityComponent(nextVelocity), lifetime,
            kinematics, ProjectileSimulationStatus.NotReady,
            "The projectile gravity direction is unresolved.");
      }

      nextVelocity = GravityIntegration.Apply(nextVelocity, gravity, deltaTime);
    }

    int substepCount = kinematics.SubstepCount;
    float substepDelta = deltaTime / substepCount;
    Vector2 position = location.Coordinates;
    bool collidedX = false;
    bool collidedY = false;
    int completedSubsteps = 0;
    bool useTiles = collisionPolicy.CollideWithTiles && tileCollision.Enabled;
    TileCollisionCapabilityComponent effectiveTileCollision =
        tileCollision with { Enabled = useTiles };

    for (int substep = 0; substep < substepCount; substep++)
    {
      SpatialTileMotionResult tileResult = useTiles
          ? frame.World.Move(position, nextVelocity * substepDelta, collider.Size,
              effectiveTileCollision, gravityDirection)
          : new SpatialTileMotionResult(position + nextVelocity * substepDelta,
              nextVelocity * substepDelta, false, false);
      if (!tileResult.IsFinite)
      {
        return Reject(frame.Revision, location, new VelocityComponent(nextVelocity), lifetime,
            kinematics, ProjectileSimulationStatus.Rejected,
            "The spatial world returned a non-finite projectile result.");
      }

      position = tileResult.Position;
      if (useTiles)
      {
        nextVelocity = tileResult.Velocity / substepDelta;
      }

      collidedX |= tileResult.CollidedX;
      collidedY |= tileResult.CollidedY;
      completedSubsteps++;
      if (tileResult.CollidedX || tileResult.CollidedY)
      {
        break;
      }
    }

    if (frame.World.Revision != frame.Revision)
    {
      return Reject(frame.Revision, location, new VelocityComponent(nextVelocity), lifetime,
          kinematics, ProjectileSimulationStatus.Conflict,
          "The spatial world changed during projectile simulation.");
    }

    ProjectileMotionResult motion = CreateMotion(
        new Terraria.Entity.LocationComponent(position),
        new VelocityComponent(nextVelocity), collidedX, collidedY, completedSubsteps);
    return ProjectileSimulationResult.Create(
        ProjectileSimulationStatus.Applied, frame.Revision, motion,
        kinematics.BeginTick(), collidedX || collidedY);
  }

  private static ProjectileSimulationResult Reject(
      SpatialWorldRevision revision,
      Terraria.Entity.LocationComponent location,
      VelocityComponent velocity,
      ProjectileLifetimeStateComponent lifetime,
      ProjectileKinematicsStateComponent kinematics,
      ProjectileSimulationStatus status,
      string detail)
  {
    return ProjectileSimulationResult.Create(status, revision,
        CreateMotion(location, velocity, false, false, 0), kinematics, false, detail);
  }

  private static ProjectileMotionResult CreateMotion(
      Terraria.Entity.LocationComponent location, VelocityComponent velocity,
      bool collidedX, bool collidedY, int completedSubsteps)
  {
    return new ProjectileMotionResult(location, velocity, collidedX, collidedY, completedSubsteps);
  }

}
