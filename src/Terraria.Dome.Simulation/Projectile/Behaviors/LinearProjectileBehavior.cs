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
    behavior.State = behavior.State with { Phase = tick };
    transform.X += velocity.X;
    transform.Y += velocity.Y;
    return true;
  }
}
