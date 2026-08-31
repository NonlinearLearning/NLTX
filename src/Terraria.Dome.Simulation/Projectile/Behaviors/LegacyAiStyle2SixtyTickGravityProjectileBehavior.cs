using System;
using Terraria.Dome.Simulation.Components;

namespace Terraria.Dome.Simulation.Projectile.Behaviors;

public sealed class LegacyAiStyle2SixtyTickGravityProjectileBehavior : IProjectileBehavior
{
  private const int GravityStartTick = 60;
  private const float GravityPerTick = 0.2f;
  private const float HorizontalDrag = 0.99f;
  private const float MaximumVerticalVelocity = 32.0f;

  public int BehaviorId => 7;

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
    float nextVelocityX = velocity.X;
    float nextVelocityY = velocity.Y;
    if (!float.IsFinite(nextTimer))
    {
      return false;
    }

    if (nextTimer >= GravityStartTick)
    {
      nextVelocityX *= HorizontalDrag;
      nextVelocityY += GravityPerTick;
    }

    nextVelocityY = MathF.Min(nextVelocityY, MaximumVerticalVelocity);
    float nextX = transform.X + nextVelocityX;
    float nextY = transform.Y + nextVelocityY;
    if (!float.IsFinite(nextVelocityX) || !float.IsFinite(nextVelocityY) ||
        !float.IsFinite(nextX) || !float.IsFinite(nextY))
    {
      return false;
    }

    velocity.X = nextVelocityX;
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
