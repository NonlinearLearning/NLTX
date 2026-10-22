using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

public sealed class NpcPreparation<TPlan> where TPlan : class
{
  private NpcPreparation(TPlan? plan, OwnerCommitResult result)
  {
    Plan = plan;
    Result = result;
  }

  public TPlan? Plan { get; }

  public OwnerCommitResult Result { get; }

  public bool IsPrepared => Plan is not null;

  public static NpcPreparation<TPlan> Prepared(TPlan plan)
  {
    ArgumentNullException.ThrowIfNull(plan);
    OwnerCommitResult result = new(OwnerCommitStatus.Applied, null, null, null);
    return new NpcPreparation<TPlan>(plan, result);
  }

  public static NpcPreparation<TPlan> Rejected(ProtocolErrorCode code, string detail)
  {
    return new NpcPreparation<TPlan>(null, OwnerCommitResult.Rejected(code, detail));
  }
}
