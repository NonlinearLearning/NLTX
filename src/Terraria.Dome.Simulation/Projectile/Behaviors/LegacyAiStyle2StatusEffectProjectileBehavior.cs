using System;
using Terraria.Dome.Simulation.Components;

namespace Terraria.Dome.Simulation.Projectile.Behaviors;

public sealed class LegacyAiStyle2StatusEffectProjectileBehavior : IProjectileBehavior
{
  public const int Id = 17;

  private const int GravityStartTick = 15;
  private const float GravityPerTick = 0.3f;
  private const float HorizontalDrag = 0.98f;
  private const float MaximumVerticalVelocity = 32.0f;

  public int BehaviorId => Id;

  public bool TryAdvance(
    ref TransformComponent transform,
    ref VelocityComponent velocity,
    ref ProjectileBehaviorComponent behavior,
    int tick)
  {
    float nextTimer = behavior.State.Primary + 1.0f;
    if (!float.IsFinite(nextTimer))
    {
      return false;
    }

    float nextVelocityX = velocity.X;
    float nextVelocityY = velocity.Y;
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
    behavior.State = new ProjectileBehaviorState(nextTimer, 0.0f, (int)nextTimer, tick);
    return true;
  }
}
