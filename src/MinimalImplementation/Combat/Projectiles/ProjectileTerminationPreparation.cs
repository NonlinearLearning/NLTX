using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

public sealed class ProjectileTerminationPreparation
{
  private ProjectileTerminationPreparation(
    ProjectileTerminationPlan? plan,
    OwnerCommitResult result,
    bool isNoOp)
  {
    Plan = plan;
    Result = result;
    IsNoOp = isNoOp;
  }

  public ProjectileTerminationPlan? Plan { get; }

  public OwnerCommitResult Result { get; }

  public bool IsNoOp { get; }

  public static ProjectileTerminationPreparation Prepared(ProjectileTerminationPlan plan)
  {
    ArgumentNullException.ThrowIfNull(plan);
    return new ProjectileTerminationPreparation(
      plan,
      new OwnerCommitResult(OwnerCommitStatus.Applied, null, null, null),
      isNoOp: false);
  }

  public static ProjectileTerminationPreparation NoOp()
  {
    return new ProjectileTerminationPreparation(
      null,
      new OwnerCommitResult(OwnerCommitStatus.Applied, null, null, null),
      isNoOp: true);
  }

  public static ProjectileTerminationPreparation Rejected(ProtocolErrorCode code, string detail)
  {
    return new ProjectileTerminationPreparation(
      null,
      OwnerCommitResult.Rejected(code, detail),
      isNoOp: false);
  }
}
