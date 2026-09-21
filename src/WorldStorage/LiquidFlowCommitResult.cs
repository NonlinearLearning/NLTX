namespace Terraria.WorldStorage;

public readonly record struct LiquidFlowCommitResult(
  bool Succeeded,
  string? FailureReason)
{
  public static LiquidFlowCommitResult Accepted => new(true, null);

  public static LiquidFlowCommitResult Rejected(string reason)
  {
    return new LiquidFlowCommitResult(false, reason);
  }
}
