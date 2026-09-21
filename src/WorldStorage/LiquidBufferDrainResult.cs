namespace Terraria.WorldStorage;

public readonly record struct LiquidBufferDrainResult(
  LiquidBufferDrainStatus Status,
  LiquidBufferEntry? Entry,
  LiquidCellWorkItemStateCommand? ActiveCommand,
  LiquidWorkQueueOperationResult? ActiveQueueResult,
  LiquidBufferCheckingIntent CheckingIntent,
  string? FailureReason)
{
  public bool HasEntry => Entry.HasValue;

  public bool Succeeded => Status == LiquidBufferDrainStatus.Enqueued;

  public bool RequiresRetry => Status == LiquidBufferDrainStatus.Rejected;

  public static LiquidBufferDrainResult Empty =>
    new(LiquidBufferDrainStatus.Empty, null, null, null,
      LiquidBufferCheckingIntent.None, null);

  public static LiquidBufferDrainResult Enqueued(
    LiquidBufferEntry entry,
    LiquidCellWorkItemStateCommand command,
    LiquidWorkQueueOperationResult activeQueueResult,
    LiquidBufferCheckingIntent checkingIntent)
  {
    return new(
      LiquidBufferDrainStatus.Enqueued,
      entry,
      command,
      activeQueueResult,
      checkingIntent,
      null);
  }

  public static LiquidBufferDrainResult Rejected(
    LiquidBufferEntry entry,
    string reason,
    LiquidWorkQueueOperationResult activeQueueResult)
  {
    return new(
      LiquidBufferDrainStatus.Rejected,
      entry,
      null,
      activeQueueResult,
      LiquidBufferCheckingIntent.None,
      reason);
  }

  public static LiquidBufferDrainResult StateConflict(
    LiquidBufferEntry entry,
    LiquidCellWorkItemStateCommand command,
    LiquidWorkQueueOperationResult activeQueueResult,
    string reason)
  {
    return new(
      LiquidBufferDrainStatus.StateConflict,
      entry,
      command,
      activeQueueResult,
      LiquidBufferCheckingIntent.None,
      reason);
  }
}
