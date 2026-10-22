using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

public sealed class ProjectilePortalTerminationPreparation
{
  private ProjectilePortalTerminationPreparation(
    ProjectilePortalTerminationPlan? plan,
    OwnerCommitResult result,
    bool isNoOp)
  {
    Plan = plan;
    Result = result;
    IsNoOp = isNoOp;
  }

  public ProjectilePortalTerminationPlan? Plan { get; }

  public OwnerCommitResult Result { get; }

  public bool IsNoOp { get; }

  public static ProjectilePortalTerminationPreparation Prepared(ProjectilePortalTerminationPlan plan)
  {
    ArgumentNullException.ThrowIfNull(plan);
    return new ProjectilePortalTerminationPreparation(
      plan,
      new OwnerCommitResult(OwnerCommitStatus.Applied, null, null, null),
      isNoOp: false);
  }

  public static ProjectilePortalTerminationPreparation NoOp()
  {
    return new ProjectilePortalTerminationPreparation(
      null,
      new OwnerCommitResult(OwnerCommitStatus.Applied, null, null, null),
      isNoOp: true);
  }

  public static ProjectilePortalTerminationPreparation Rejected(ProtocolErrorCode code, string detail)
  {
    return new ProjectilePortalTerminationPreparation(
      null,
      OwnerCommitResult.Rejected(code, detail),
      isNoOp: false);
  }
}
