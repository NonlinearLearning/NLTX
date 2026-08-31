using System;
using Terraria.Dome.Simulation.Components;

namespace Terraria.Dome.Simulation.Projectile.Behaviors;

public sealed class LegacyAiStyle2ImmediateGravityProjectileBehavior : IProjectileBehavior
{
  private const float GravityPerTick = 0.25f;
  private const float MaximumVerticalVelocity = 32.0f;

  public int BehaviorId => 5;

  public bool TryAdvance(
    ref TransformComponent transform,
    ref VelocityComponent velocity,
    ref ProjectileBehaviorComponent behavior,
    int tick)
  {
    if (!float.IsFinite(transform.X) || !float.IsFinite(transform.Y) ||
        !float.IsFinite(velocity.X) || !float.IsFinite(velocity.Y) ||
        !float.IsFinite(behavior.State.Primary) || behavior.State.Primary < 0.0f || tick < 0)
    {
      return false;
    }

    float nextTimer = behavior.State.Primary + 1.0f;
    float nextVelocityY = MathF.Min(velocity.Y + GravityPerTick, MaximumVerticalVelocity);
    float nextX = transform.X + velocity.X;
    float nextY = transform.Y + nextVelocityY;
    if (!float.IsFinite(nextTimer) || !float.IsFinite(nextVelocityY) ||
        !float.IsFinite(nextX) || !float.IsFinite(nextY))
    {
      return false;
    }

    velocity.Y = nextVelocityY;
    transform.X = nextX;
    transform.Y = nextY;
    behavior.State = behavior.State with
    {
      Primary = nextTimer,
      Phase = tick
    };
    return true;
  }
}
