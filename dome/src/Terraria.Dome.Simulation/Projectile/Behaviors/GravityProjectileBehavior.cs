using Terraria.Dome.Simulation.Components;

using EntityEcs.Components;
namespace Terraria.Dome.Simulation.Projectile.Behaviors;

public sealed class GravityProjectileBehavior : IProjectileBehavior
{
  private const float GravityPerTick = -0.25f;

  public int BehaviorId => 2;

  public bool TryAdvance(
    ref LocationComponent transform,
    ref VelocityComponent velocity,
    ref ProjectileBehaviorComponent behavior,
    int tick)
  {
    if (!float.IsFinite(transform.X) || !float.IsFinite(transform.Y) ||
        !float.IsFinite(velocity.X) || !float.IsFinite(velocity.Y) || tick < 0)
    {
      return false;
    }

    float nextVelocityY = velocity.Y + GravityPerTick;
    float nextX = transform.X + velocity.X;
    float nextY = transform.Y + nextVelocityY;
    if (!float.IsFinite(nextVelocityY) || !float.IsFinite(nextX) || !float.IsFinite(nextY))
    {
      return false;
    }

    velocity.Y = nextVelocityY;
    transform.X = nextX;
    transform.Y = nextY;
    behavior.State = behavior.State with
    {
      Primary = velocity.Y,
      Phase = tick
    };
    return true;
  }
}
