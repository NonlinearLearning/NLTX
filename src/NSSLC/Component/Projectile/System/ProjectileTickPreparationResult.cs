namespace Terraria.Projectile;

/// <summary>
/// Describes how the pre-AI prefix ended before the shared sound-delay decrement.
/// </summary>
public enum ProjectileTickPreparationResult
{
  /// <summary>The prefix reached the pre-AI boundary.</summary>
  Ready,

  /// <summary>A legacy continue skipped AI, cooldown, and the lifetime tail.</summary>
  Continued,

  /// <summary>A legacy return ended this projectile update before AI.</summary>
  Returned,

  /// <summary>The lifecycle owner must deactivate a projectile outside world bounds.</summary>
  WorldBoundaryDeactivation,

  /// <summary>A minion or sentry branch requires a Player owner adapter.</summary>
  IntegrationRequired,
}
