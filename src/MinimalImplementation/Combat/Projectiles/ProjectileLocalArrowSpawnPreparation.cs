using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

public sealed class ProjectileLocalArrowSpawnPreparation
{
  private ProjectileLocalArrowSpawnPreparation(
      ProjectileLocalArrowSpawnPlan? plan,
      OwnerCommitResult result)
  {
    Plan = plan;
    Result = result;
  }

  public ProjectileLocalArrowSpawnPlan? Plan { get; }

  public OwnerCommitResult Result { get; }

  public bool IsPrepared => Plan is not null;

  internal static ProjectileLocalArrowSpawnPreparation Prepared(
      ProjectileLocalArrowSpawnPlan plan)
  {
    ArgumentNullException.ThrowIfNull(plan);
    return new ProjectileLocalArrowSpawnPreparation(
        plan, OwnerCommitResult.Applied(plan.WorldCommitVersion));
  }

  internal static ProjectileLocalArrowSpawnPreparation Rejected(
      ProtocolErrorCode code,
      string detail)
  {
    return new ProjectileLocalArrowSpawnPreparation(
        null, OwnerCommitResult.Rejected(code, detail));
  }
}
