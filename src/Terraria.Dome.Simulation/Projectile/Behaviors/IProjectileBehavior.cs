using Terraria.Dome.Simulation.Components;

namespace Terraria.Dome.Simulation.Projectile.Behaviors;

public interface IProjectileBehavior
{
  int BehaviorId { get; }

  bool TryAdvance(
    ref TransformComponent transform,
    ref VelocityComponent velocity,
    ref ProjectileBehaviorComponent behavior,
    int tick);
}
