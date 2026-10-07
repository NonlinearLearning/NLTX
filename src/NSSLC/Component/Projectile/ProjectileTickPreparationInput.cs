using EntityEcs.Components;

namespace Terraria.Projectile;

/// <summary>
/// The projectile capabilities read by the confirmed update-prefix rules.
/// This value is captured for one substep and does not own component state.
/// </summary>
public readonly record struct ProjectileTickPreparationInput
{
  public ProjectileTickPreparationInput(
    ProjectileDefinitionComponent definition,
    ProjectileBehaviorStateComponent behavior,
    ProjectileTrajectoryStateComponent trajectory,
    LocationComponent location,
    VelocityComponent velocity,
    ColliderComponent collider,
    bool hasMinionCapability,
    ProjectileMinionCapabilityComponent minion,
    bool hasSentryCapability,
    ProjectileSentryCapabilityComponent sentry)
  {
    Definition = definition;
    Behavior = behavior;
    Trajectory = trajectory;
    Location = location;
    Velocity = velocity;
    Collider = collider;
    HasMinionCapability = hasMinionCapability;
    Minion = minion;
    HasSentryCapability = hasSentryCapability;
    Sentry = sentry;
  }

  public ProjectileDefinitionComponent Definition { get; }

  public ProjectileBehaviorStateComponent Behavior { get; }

  public ProjectileTrajectoryStateComponent Trajectory { get; }

  public LocationComponent Location { get; }

  public VelocityComponent Velocity { get; }

  public ColliderComponent Collider { get; }

  public bool HasMinionCapability { get; }

  public ProjectileMinionCapabilityComponent Minion { get; }

  public bool HasSentryCapability { get; }

  public ProjectileSentryCapabilityComponent Sentry { get; }
}
