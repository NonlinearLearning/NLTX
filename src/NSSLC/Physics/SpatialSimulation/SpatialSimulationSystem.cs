using System;
using System.Numerics;
using Terraria.Liquid;
using Terraria.Movement;

namespace Terraria.Physics;

/// <summary>
/// Calculates one entity's movement from detached state and a spatial frame. The system does
/// not access ECS storage and does not commit any component.
/// </summary>
public sealed class SpatialSimulationSystem
{
  public SpatialSimulationResult<TEntity> Simulate<TEntity>(
      in SpatialFrame frame,
      in SpatialEntitySnapshot<TEntity> entity,
      in MovementCapabilityComponent movement,
      in GravityCapabilityComponent gravityCapability,
      in GravityStateComponent gravity,
      in TileCollisionCapabilityComponent tileCollision,
      in LiquidInteractionCapabilityComponent liquidInteraction,
      in MovementIntent intent,
      float deltaTime)
      where TEntity : notnull
  {
    if (intent.Tick != frame.Tick)
    {
      return SpatialSimulationResult<TEntity>.Rejected(
          entity.Entity,
          frame.Revision,
          SpatialSimulationStatus.Conflict,
          "The movement intent belongs to another simulation tick.");
    }

    if (!float.IsFinite(deltaTime) || deltaTime <= 0f)
    {
      return SpatialSimulationResult<TEntity>.Rejected(
          entity.Entity,
          frame.Revision,
          SpatialSimulationStatus.NotReady,
          "The spatial delta time must be finite and positive.");
    }

    if (!entity.Collider.Enabled || !movement.Enabled)
    {
      return SpatialSimulationResult<TEntity>.Rejected(
          entity.Entity,
          frame.Revision,
          SpatialSimulationStatus.NotReady,
          "The entity does not have an enabled movement or collider capability.");
    }

    if (gravity.Direction is not (GravityDirection.Normal or GravityDirection.Reversed))
    {
      return SpatialSimulationResult<TEntity>.Rejected(
          entity.Entity,
          frame.Revision,
          SpatialSimulationStatus.NotReady,
          "The spatial gravity direction is not resolved.");
    }

    Vector2 velocity = movement.Mode == MovementMode.Flight || movement.Mode == MovementMode.Free
        ? ResolveFreeVelocity(entity.Velocity.LinearVelocity, intent, movement, deltaTime)
        : ResolveGroundVelocity(entity.Velocity.LinearVelocity, intent, movement, deltaTime);
    bool jumpConsumed = intent.JumpRequested && movement.CanJump && entity.Grounding.IsGrounded;
    if (jumpConsumed)
    {
      float jumpSpeed = intent.JumpSpeed > 0f ? intent.JumpSpeed : movement.JumpSpeed;
      velocity.Y = -jumpSpeed * (gravity.Direction == GravityDirection.Reversed ? -1f : 1f);
    }

    if (gravityCapability.Enabled && movement.Mode != MovementMode.Flight)
    {
      velocity = GravityIntegration.Apply(velocity, gravity, deltaTime);
    }

    SpatialTileMotionResult motion = tileCollision.Enabled
        ? frame.World.Move(
            entity.Location.Coordinates,
            velocity * deltaTime,
            entity.Collider.Size,
            tileCollision,
            gravity.Direction)
        : new SpatialTileMotionResult(
            entity.Location.Coordinates + velocity * deltaTime,
            velocity * deltaTime,
            false,
            false);
    if (!motion.IsFinite)
    {
      return SpatialSimulationResult<TEntity>.Rejected(
          entity.Entity,
          frame.Revision,
          SpatialSimulationStatus.Rejected,
          "The world returned a non-finite spatial result.");
    }

    SpatialLiquidContact liquid = liquidInteraction.Enabled
        ? frame.World.GetLiquidContact(
            motion.Position,
            entity.Collider.Size,
            liquidInteraction)
        : new SpatialLiquidContact(LiquidContactFlags.None, 0);
    GroundingComponent grounding = ResolveGrounding(motion, gravity);
    var commit = new SpatialCommit(
        new Terraria.Entity.LocationComponent(motion.Position),
        new VelocityComponent(motion.Velocity / deltaTime),
        grounding,
        liquid.ToComponent());
    return SpatialSimulationResult<TEntity>.Applied(
        entity.Entity,
        frame.Revision,
        commit,
        jumpConsumed);
  }

  private static Vector2 ResolveGroundVelocity(
      Vector2 current,
      MovementIntent intent,
      MovementCapabilityComponent movement,
      float deltaTime)
  {
    Vector2 desired = intent.HasDesiredVelocity ? intent.DesiredVelocity : current;
    if (intent.Source == MovementIntentSource.Ai)
    {
      // NPC AI owns horizontal steering. Vertical motion remains owned by gravity and the
      // collision step; forcing an AI's usual y=0 target would cancel a fall every tick.
      desired.Y = current.Y;
    }
    float maximum = movement.MaximumSpeed;
    desired.X = Math.Clamp(desired.X, -maximum, maximum);
    desired.Y = Math.Clamp(desired.Y, -maximum, maximum);
    return Terraria.Physics.SimulationVector.MoveTowards(current, desired, movement.Acceleration * deltaTime);
  }

  private static Vector2 ResolveFreeVelocity(
      Vector2 current,
      MovementIntent intent,
      MovementCapabilityComponent movement,
      float deltaTime)
  {
    Vector2 desired = intent.HasDesiredVelocity ? intent.DesiredVelocity : current;
    if (movement.MaximumSpeed > 0f && desired.LengthSquared() > movement.MaximumSpeed * movement.MaximumSpeed)
    {
      desired = Vector2.Normalize(desired) * movement.MaximumSpeed;
    }

    return Terraria.Physics.SimulationVector.MoveTowards(current, desired, movement.Acceleration * deltaTime);
  }

  private static GroundingComponent ResolveGrounding(
      SpatialTileMotionResult motion,
      GravityStateComponent gravity)
  {
    bool grounded = motion.CollidedY;
    Vector2 normal = grounded
        ? gravity.Direction == GravityDirection.Reversed ? -Vector2.UnitY : Vector2.UnitY
        : Vector2.Zero;
    return new GroundingComponent(grounded, false, normal);
  }
}
