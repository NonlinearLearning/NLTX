using Terraria.Dome.Simulation.Components;

namespace Terraria.Dome.Simulation.Projectile.Behaviors;

public sealed class LinearProjectileBehavior : IProjectileBehavior
{
  public int BehaviorId => 1;

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

    behavior.State = behavior.State with { Phase = tick };
    transform.X = nextX;
    transform.Y = nextY;
    return true;
  }
}
