using Terraria.Dome.Simulation.Components;

namespace Terraria.Dome.Simulation.Projectile.Behaviors;

public sealed class LegacyAiStyle29ParentProjectileBehavior : IProjectileBehavior
{
  public const int Id = 11;

  public int BehaviorId => Id;

  public bool TryAdvance(
    ref TransformComponent transform,
    ref VelocityComponent velocity,
    ref ProjectileBehaviorComponent behavior,
    int tick)
  {
    if (!float.IsFinite(transform.X) || !float.IsFinite(transform.Y) ||
        !float.IsFinite(velocity.X) || !float.IsFinite(velocity.Y) || tick < 0)
    {
      return false;
    }

    float nextX = transform.X + velocity.X;
    float nextY = transform.Y + velocity.Y;
    if (!float.IsFinite(nextX) || !float.IsFinite(nextY))
    {
      return false;
    }

    transform.X = nextX;
    transform.Y = nextY;
    behavior.State = behavior.State with { Phase = tick };
    return true;
  }
}
