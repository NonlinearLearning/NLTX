using EntityEcs.Components;

namespace Terraria.Projectile;

/// <summary>
/// The bounded component changes produced by one projectile preparation step.
/// </summary>
public readonly record struct ProjectileTickPreparationChanges
{
  public ProjectileTickPreparationChanges(
    float? behaviorAi1 = null,
    float? trajectoryGfxOffY = null,
    VelocityComponent? previousVelocity = null)
  {
    BehaviorAi1 = behaviorAi1;
    TrajectoryGfxOffY = trajectoryGfxOffY;
    PreviousVelocity = previousVelocity;
  }

  public float? BehaviorAi1 { get; }

  public float? TrajectoryGfxOffY { get; }

  public VelocityComponent? PreviousVelocity { get; }
}
