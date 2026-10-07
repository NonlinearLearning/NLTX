namespace Terraria.WorldStorage;

public readonly record struct LiquidWorkQueueOperationResult(
  bool Succeeded,
  bool Changed,
  bool WasDuplicate,
  bool WasRemoved,
  string? FailureReason)
{
  public static LiquidWorkQueueOperationResult Accepted(bool changed)
  {
    return new LiquidWorkQueueOperationResult(
      true,
      changed,
      false,
      false,
      null);
  }

  public static LiquidWorkQueueOperationResult Duplicate =>
    new(true, false, true, false, null);

  public static LiquidWorkQueueOperationResult Removed =>
    new(true, true, false, true, null);

  public static LiquidWorkQueueOperationResult Rejected(string reason)
  {
    return new LiquidWorkQueueOperationResult(
      false,
      false,
      false,
      false,
      reason);
  }
}
