namespace Terraria.WorldStorage;

public readonly record struct LiquidPublicationProjectionResult(
  LiquidPublicationProjectionStatus Status,
  LiquidChangePublicationSnapshot? Snapshot,
  string? FailureReason)
{
  public bool HasSnapshot => Snapshot is not null;

  public bool Succeeded => Status == LiquidPublicationProjectionStatus.Ready ||
    Status == LiquidPublicationProjectionStatus.Acknowledged;

  public static LiquidPublicationProjectionResult Empty(long revision)
  {
    return new(
      LiquidPublicationProjectionStatus.Empty,
      LiquidChangePublicationSnapshot.Empty(revision),
      null);
  }

  public static LiquidPublicationProjectionResult Suppressed(long revision)
  {
    return new(
      LiquidPublicationProjectionStatus.Suppressed,
      LiquidChangePublicationSnapshot.Empty(revision),
      "publication-disabled");
  }

  public static LiquidPublicationProjectionResult Ready(
    LiquidChangePublicationSnapshot snapshot)
  {
    return new(
      LiquidPublicationProjectionStatus.Ready,
      snapshot,
      null);
  }

  public static LiquidPublicationProjectionResult InFlight(
    LiquidChangePublicationSnapshot snapshot)
  {
    return new(
      LiquidPublicationProjectionStatus.InFlight,
      snapshot,
      "publication-in-flight");
  }

  public static LiquidPublicationProjectionResult Acknowledged(
    long revision)
  {
    return new(
      LiquidPublicationProjectionStatus.Acknowledged,
      LiquidChangePublicationSnapshot.Empty(revision),
      null);
  }

  public static LiquidPublicationProjectionResult Rejected(string reason)
  {
    return new(
      LiquidPublicationProjectionStatus.Rejected,
      null,
      reason);
  }
}
