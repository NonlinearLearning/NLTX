using Terraria.Dome.Simulation.Components;

using EntityEcs.Components;
namespace Terraria.Dome.Simulation.Projectile.Behaviors;

public sealed class LegacyAiStyle17ProjectileBehavior : IProjectileBehavior
{
  public const int Id = 20;

  private const float GravityPerTick = 0.2f;
  private const float HorizontalDragOnGround = 0.98f;

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

    float nextVelocityX = velocity.Y == 0.0f
      ? velocity.X * HorizontalDragOnGround
      : velocity.X;
    float nextVelocityY = velocity.Y + GravityPerTick;
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
    behavior.State = behavior.State with { Phase = behavior.State.Phase + 1 };
    return true;
  }
}
