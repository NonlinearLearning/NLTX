namespace Terraria.Projectile;

/// <summary>
/// Describes how the post-preparation portion of a projectile substep ended.
/// </summary>
public enum ProjectileTickSubstepResult
{
  /// <summary>The body reached the time-left and penetration tail.</summary>
  Completed,

  /// <summary>A legacy continue skipped the tail and proceeds to the next substep.</summary>
  Continued,

  /// <summary>A legacy return skipped the tail and ends this projectile update.</summary>
  Returned,
}
