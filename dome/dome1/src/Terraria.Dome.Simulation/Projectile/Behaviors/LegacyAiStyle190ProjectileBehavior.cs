using Terraria.Dome.Simulation.Components;

using EntityEcs.Components;
namespace Terraria.Dome.Simulation.Projectile.Behaviors;

public sealed class LegacyAiStyle190ProjectileBehavior : IProjectileBehavior
{
  public const int Id = 19;

  public int BehaviorId => Id;

  public bool TryAdvance(
    ref LocationComponent transform,
    ref VelocityComponent velocity,
    ref ProjectileBehaviorComponent behavior,
    int tick)
  {
    if (!float.IsFinite(transform.X) || !float.IsFinite(transform.Y) ||
        !float.IsFinite(velocity.X) || !float.IsFinite(velocity.Y) ||
        !float.IsFinite(behavior.State.Primary) || !float.IsFinite(behavior.State.Secondary) ||
        !float.IsFinite(behavior.State.Tertiary) || behavior.State.Secondary <= 0.0f ||
        behavior.State.Phase == int.MaxValue || tick < 0)
    {
      return false;
    }

    behavior.State = behavior.State with { Phase = behavior.State.Phase + 1 };
    return true;
  }
}
