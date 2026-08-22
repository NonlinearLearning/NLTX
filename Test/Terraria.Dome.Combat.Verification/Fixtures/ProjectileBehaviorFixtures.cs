using System;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Projectile.Behaviors;

namespace Terraria.Dome.Combat.Verification.Fixtures;

public static class ProjectileBehaviorFixtures
{
  public static void VerifyLinear()
  {
    LinearProjectileBehavior behavior = new();
    TransformComponent transform = new(1.0f, 2.0f);
    VelocityComponent velocity = new(4.0f, 0.0f);
    ProjectileBehaviorComponent state = new(
      behavior.BehaviorId,
      new ProjectileBehaviorState(0.0f, 0.0f, 0, 0));
    if (!behavior.TryAdvance(ref transform, ref velocity, ref state, 1) ||
        transform.X != 5.0f || transform.Y != 2.0f ||
        state.State.Phase != 1)
    {
      throw new InvalidOperationException("Linear projectile fixture diverged at tick one.");
    }

    TransformComponent invalidTransform = new(float.NaN, 2.0f);
    if (behavior.TryAdvance(ref invalidTransform, ref velocity, ref state, 2))
    {
      throw new InvalidOperationException("Linear projectile behavior accepted a non-finite transform.");
    }

    if (behavior.TryAdvance(ref transform, ref velocity, ref state, -1))
    {
      throw new InvalidOperationException("Linear projectile behavior accepted a negative tick.");
    }
  }

  public static void VerifyGravity()
  {
    GravityProjectileBehavior behavior = new();
    TransformComponent transform = new(1.0f, 2.0f);
    VelocityComponent velocity = new(4.0f, 1.0f);
    ProjectileBehaviorComponent state = new(
      behavior.BehaviorId,
      new ProjectileBehaviorState(0.0f, 0.0f, 0, 0));
    if (!behavior.TryAdvance(ref transform, ref velocity, ref state, 1) ||
        transform.X != 5.0f || transform.Y != 2.75f ||
        velocity.Y != 0.75f || state.State.Primary != 0.75f)
    {
      throw new InvalidOperationException("Gravity projectile fixture diverged at tick one.");
    }

    VelocityComponent invalidVelocity = new(float.PositiveInfinity, 1.0f);
    if (behavior.TryAdvance(ref transform, ref invalidVelocity, ref state, 2))
    {
      throw new InvalidOperationException("Gravity projectile behavior accepted a non-finite velocity.");
    }
  }
}
