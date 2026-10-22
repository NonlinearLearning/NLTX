using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

public sealed class ProjectileMovementPreparation {
  private ProjectileMovementPreparation(ProjectileMovementPlan? plan, OwnerCommitResult result) {
    Plan = plan;
    Result = result;
  }

  public ProjectileMovementPlan? Plan { get; }

  public OwnerCommitResult Result { get; }

  public bool IsPrepared => Plan is not null && Result.Status == OwnerCommitStatus.Applied;

  internal static ProjectileMovementPreparation Prepared(ProjectileMovementPlan plan) {
    return new ProjectileMovementPreparation(
        plan, new OwnerCommitResult(OwnerCommitStatus.Applied, null, null, null));
  }

  internal static ProjectileMovementPreparation Rejected(ProtocolErrorCode code, string detail) {
    return new ProjectileMovementPreparation(null, OwnerCommitResult.Rejected(code, detail));
  }
}
