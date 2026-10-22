using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

public sealed class ProjectileSyncPreparation
{
  private ProjectileSyncPreparation(ProjectileSyncPlan? plan, OwnerCommitResult result)
  {
    Plan = plan;
    Result = result;
  }

  public ProjectileSyncPlan? Plan { get; }

  public OwnerCommitResult Result { get; }

  public bool IsPrepared => Plan is not null;

  public static ProjectileSyncPreparation Prepared(ProjectileSyncPlan plan)
  {
    ArgumentNullException.ThrowIfNull(plan);
    return new ProjectileSyncPreparation(
      plan,
      new OwnerCommitResult(OwnerCommitStatus.Applied, null, null, null));
  }

  public static ProjectileSyncPreparation Rejected(ProtocolErrorCode code, string detail)
  {
    return new ProjectileSyncPreparation(null, OwnerCommitResult.Rejected(code, detail));
  }
}
