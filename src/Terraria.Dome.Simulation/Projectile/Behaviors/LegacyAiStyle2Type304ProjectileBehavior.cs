using System;
using Terraria.Dome.Simulation.Components;

namespace Terraria.Dome.Simulation.Projectile.Behaviors;

public sealed class LegacyAiStyle2Type304ProjectileBehavior : IProjectileBehavior
{
  public const int Id = 16;

  private const float MaximumVerticalVelocity = 32.0f;

  public int BehaviorId => Id;

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
    float nextLocalAi0 = behavior.State.LocalAi0 + 1.0f;
    float nextX = transform.X + velocity.X;
    float nextVelocityY = MathF.Min(velocity.Y, MaximumVerticalVelocity);
    float nextY = transform.Y + nextVelocityY;
    if (!float.IsFinite(nextTimer) || !float.IsFinite(nextLocalAi0) ||
        !float.IsFinite(nextX) || !float.IsFinite(nextY))
    {
      return false;
    }

    transform.X = nextX;
    transform.Y = nextY;
    velocity.Y = nextVelocityY;
    behavior.State = behavior.State with
    {
      Primary = nextTimer,
      Phase = tick,
      LocalAi0 = nextLocalAi0
    };
    return true;
  }
}
