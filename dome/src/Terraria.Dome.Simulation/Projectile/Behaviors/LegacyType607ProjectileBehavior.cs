using Terraria.Dome.Simulation.Components;

using EntityEcs.Components;
namespace Terraria.Dome.Simulation.Projectile.Behaviors;

public sealed class LegacyType607ProjectileBehavior : IProjectileBehavior
{
  public const int Id = 21;

  public int BehaviorId => Id;

  public bool TryAdvance(
    ref LocationComponent transform,
    ref VelocityComponent velocity,
    ref ProjectileBehaviorComponent behavior,
    int tick)
  {
    if (!float.IsFinite(transform.X) || !float.IsFinite(transform.Y) ||
        !float.IsFinite(velocity.X) || !float.IsFinite(velocity.Y) || tick < 0 ||
        behavior.State.Phase == int.MaxValue)
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
    behavior.State = behavior.State with { Phase = behavior.State.Phase + 1 };
    return true;
  }
}
