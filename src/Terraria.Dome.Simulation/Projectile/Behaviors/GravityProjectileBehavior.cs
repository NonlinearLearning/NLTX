using Terraria.Dome.Simulation.Components;

namespace Terraria.Dome.Simulation.Projectile.Behaviors;

public sealed class GravityProjectileBehavior : IProjectileBehavior
{
  private const float GravityPerTick = -0.25f;

  public int BehaviorId => 2;

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

    velocity.Y += GravityPerTick;
    transform.X += velocity.X;
    transform.Y += velocity.Y;
    behavior.State = behavior.State with
    {
      Primary = velocity.Y,
      Phase = tick
    };
    return true;
  }
}
