namespace Terraria.Projectile;

/// <summary>
/// Snapshot of tile-dependent predicates consumed by projectile collision geometry.
/// </summary>
public readonly record struct ProjectileCollisionEnvironmentSnapshot
{
  public ProjectileCollisionEnvironmentSnapshot(
    bool hasCanHitResult,
    bool canHit,
    bool hasCanHitLineResult,
    bool canHitLine,
    ProjectileAi137VisibilitySnapshot ai137Visibility)
  {
    HasCanHitResult = hasCanHitResult;
    CanHit = canHit;
    HasCanHitLineResult = hasCanHitLineResult;
    CanHitLine = canHitLine;
    Ai137Visibility = ai137Visibility;
  }

  public bool HasCanHitResult { get; }

  public bool CanHit { get; }

  public bool HasCanHitLineResult { get; }

  public bool CanHitLine { get; }

  public ProjectileAi137VisibilitySnapshot Ai137Visibility { get; }
}
