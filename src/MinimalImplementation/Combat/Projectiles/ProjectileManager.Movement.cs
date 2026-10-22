using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

public sealed partial class ProjectileManager {
  internal ProjectileCollisionCommitResult CommitArrowMovement(ProjectileMovementPlan plan) {
    ProjectileView expected = plan.Source;
    if (!plan.TileHit) {
      var committed = CommitTick(
          expected, plan.Position, plan.Velocity, plan.Ai0,
          expected.Ai1, expected.Ai2, expected.TimeLeft, plan.LiquidState, plan.ExtraUpdates);
      return new ProjectileCollisionCommitResult(
          committed.Result, committed.Projectile, null, null);
    }

    if (!TryGet(expected.Entity, out ProjectileView? current) || current is null ||
        current.Revision != expected.Revision || current.CommitVersion != expected.CommitVersion) {
      return RejectedCollision(
          ProtocolErrorCode.StaleEntity,
          "The Arrow tile movement generation or revision is stale.");
    }
    if (current.Revision == long.MaxValue || !TryGetNextCommit(out CommitVersion nextVersion)) {
      return RejectedCollision(
          ProtocolErrorCode.NotReady, "Arrow tile movement commit version range is exhausted.");
    }
    Projectile projectile = _byOwnerIdentity[(current.Owner, current.ProtocolIdentity)];
    projectile.ApplyTick(
        plan.Position, plan.Velocity, plan.Ai0,
        current.Ai1, current.Ai2, current.TimeLeft,
        plan.LiquidState, plan.ExtraUpdates, nextVersion);
    ProjectileView retired = projectile.Capture();
    if (!RetireProjectile(projectile)) {
      // Identity rejection restores motion/liquid/update state; manager version stays unchanged.
      // Capture current dirty immediately before ApplyTick, not at preparation time.
      projectile.RestoreTickCommit(current);
      return RejectedCollision(
          ProtocolErrorCode.StaleEntity, "The identity authority rejected Arrow tile retirement.");
    }
    CommitMutation(nextVersion);
    OwnerCommitResult? cleanup = _trackers?.RemoveEntity(retired.Entity);
    return new ProjectileCollisionCommitResult(
        OwnerCommitResult.Applied(nextVersion), null, retired, cleanup);
  }
}
