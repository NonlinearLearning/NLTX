namespace MinimalImplementation.Combat.Projectiles;

/// <summary>
/// One Arrow frame's sequencing receipt, not an authoritative projectile store.
/// UpdateCount is fixed at entry. After a consumed NPC hit, discard this receipt;
/// remaining budget does not imply that its generation is still active.
/// </summary>
public sealed class ProjectileArrowFrame {
  internal ProjectileArrowFrame(ProjectileMovementService owner, ProjectileView source) {
    Owner = owner;
    Source = source;
    ExpectedProjectile = source;
    UpdateCount = source.ExtraUpdates + 1;
  }

  public ProjectileView Source { get; }

  public int UpdateCount { get; }

  public int CompletedUpdates { get; internal set; }

  public bool Terminated { get; internal set; }

  internal ProjectileMovementService Owner { get; }

  internal ProjectileView ExpectedProjectile { get; set; }

  internal ProjectileMovementPlan? PendingMovement { get; set; }
}
