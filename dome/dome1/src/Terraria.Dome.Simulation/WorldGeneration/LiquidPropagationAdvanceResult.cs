namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LiquidPropagationAdvanceResult(
  bool Succeeded,
  int ConsumedWorkItems,
  int PendingWorkItems,
  string? FailureReason)
{
  public static LiquidPropagationAdvanceResult Failed(string reason, int pendingWorkItems)
  {
    return new LiquidPropagationAdvanceResult(false, 0, pendingWorkItems, reason);
  }
}
