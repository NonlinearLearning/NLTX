namespace Terraria.WorldStorage;

public readonly record struct LiquidNetworkPublishResult(
  LiquidNetworkPublishStatus Status,
  LiquidChangePublicationSnapshot? Snapshot,
  string? FailureReason)
{
  public bool Succeeded => Status == LiquidNetworkPublishStatus.Delivered;

  public bool HasSnapshot => Snapshot is not null;

  public static LiquidNetworkPublishResult FromProjection(
    LiquidPublicationProjectionResult projection)
  {
    LiquidNetworkPublishStatus status = projection.Status switch
    {
      LiquidPublicationProjectionStatus.Empty => LiquidNetworkPublishStatus.Empty,
      LiquidPublicationProjectionStatus.Suppressed => LiquidNetworkPublishStatus.Suppressed,
      LiquidPublicationProjectionStatus.InFlight => LiquidNetworkPublishStatus.InFlight,
      LiquidPublicationProjectionStatus.Rejected => LiquidNetworkPublishStatus.Rejected,
      _ => LiquidNetworkPublishStatus.Rejected
    };
    return new(status, projection.Snapshot, projection.FailureReason);
  }
}
