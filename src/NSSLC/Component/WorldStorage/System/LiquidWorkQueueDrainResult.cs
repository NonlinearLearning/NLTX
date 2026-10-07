namespace Terraria.WorldStorage;

public readonly record struct LiquidWorkQueueDrainResult(
  LiquidWorkQueueDrainStatus Status,
  LiquidCellWorkItemStateCommand? Item,
  string? FailureReason)
{
  public bool HasItem => Item.HasValue;

  public static LiquidWorkQueueDrainResult Empty =>
    new(LiquidWorkQueueDrainStatus.Empty, null, null);

  public static LiquidWorkQueueDrainResult Ready(
    LiquidCellWorkItemStateCommand item)
  {
    return new(LiquidWorkQueueDrainStatus.Ready, item, null);
  }

  public static LiquidWorkQueueDrainResult Deferred(
    LiquidCellWorkItemStateCommand item)
  {
    return new(LiquidWorkQueueDrainStatus.Deferred, item, null);
  }

  public static LiquidWorkQueueDrainResult Removed(
    LiquidCellWorkItemStateCommand item)
  {
    return new(LiquidWorkQueueDrainStatus.Removed, item, null);
  }

  public static LiquidWorkQueueDrainResult Rejected(string reason)
  {
    return new(LiquidWorkQueueDrainStatus.Rejected, null, reason);
  }
}
