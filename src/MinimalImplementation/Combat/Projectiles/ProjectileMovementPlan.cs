using System.Numerics;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

/// <summary>Detached Arrow calculation, bound to its service and world snapshot.</summary>
public sealed class ProjectileMovementPlan {
  internal ProjectileMovementPlan(
      ProjectileMovementService owner, long managerRevision, ProjectileView source,
      IProjectileMotionSnapshot snapshot, ulong worldEpoch, CommitVersion worldCommitVersion,
      Vector2 position, Vector2 velocity, float ai0, bool tileHit,
      ProjectileLiquidState liquidState, int extraUpdates, ProjectileArrowFrame frame) {
    Owner = owner;
    ManagerRevision = managerRevision;
    Source = source;
    Snapshot = snapshot;
    WorldEpoch = worldEpoch;
    WorldCommitVersion = worldCommitVersion;
    Position = position;
    Velocity = velocity;
    Ai0 = ai0;
    TileHit = tileHit;
    LiquidState = liquidState;
    ExtraUpdates = extraUpdates;
    Frame = frame;
  }

  public ProjectileView Source { get; }

  public ulong WorldEpoch { get; }

  public CommitVersion WorldCommitVersion { get; }

  public Vector2 Position { get; }

  public Vector2 Velocity { get; }

  public float Ai0 { get; }

  public bool TileHit { get; }

  public ProjectileLiquidState LiquidState { get; }

  public int ExtraUpdates { get; }

  internal ProjectileMovementService Owner { get; }

  internal long ManagerRevision { get; }

  internal IProjectileMotionSnapshot Snapshot { get; }

  internal ProjectileArrowFrame Frame { get; }

  // A receipt for CompleteTick, never a second authoritative motion store.
  internal ProjectileView? MovedProjectile { get; set; }
}
