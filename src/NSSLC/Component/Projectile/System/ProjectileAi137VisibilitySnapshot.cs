namespace Terraria.Projectile;

/// <summary>
/// Visibility results computed for the two target points tested by AI style 137.
/// </summary>
public readonly record struct ProjectileAi137VisibilitySnapshot
{
  public ProjectileAi137VisibilitySnapshot(
    bool hasTargetCenterResult,
    bool targetCenterCanHit,
    bool hasTargetTopCenterResult,
    bool targetTopCenterCanHit)
  {
    HasTargetCenterResult = hasTargetCenterResult;
    TargetCenterCanHit = targetCenterCanHit;
    HasTargetTopCenterResult = hasTargetTopCenterResult;
    TargetTopCenterCanHit = targetTopCenterCanHit;
  }

  public bool HasTargetCenterResult { get; }

  public bool TargetCenterCanHit { get; }

  public bool HasTargetTopCenterResult { get; }

  public bool TargetTopCenterCanHit { get; }

  public bool HasResults => HasTargetCenterResult &&
    (TargetCenterCanHit || HasTargetTopCenterResult);
}
