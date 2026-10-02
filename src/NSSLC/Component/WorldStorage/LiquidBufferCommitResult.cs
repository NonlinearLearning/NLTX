namespace Terraria.WorldStorage;

public readonly record struct LiquidBufferCommitResult(
  LiquidBufferCommitStatus Status,
  bool Changed,
  TileCoordinate? Coordinate,
  LiquidBufferCheckingIntent CheckingIntent,
  string? FailureReason)
{
  public bool Succeeded => Status != LiquidBufferCommitStatus.Rejected;

  public bool WasDuplicate => Status == LiquidBufferCommitStatus.Duplicate;

  public bool HasCheckingIntent => CheckingIntent.Kind !=
    LiquidBufferCheckingIntentKind.None;

  public static LiquidBufferCommitResult Accepted(
    TileCoordinate coordinate)
  {
    return new LiquidBufferCommitResult(
      LiquidBufferCommitStatus.Accepted,
      true,
      coordinate,
      LiquidBufferCheckingIntent.Set(coordinate),
      null);
  }

  public static LiquidBufferCommitResult Duplicate(
    TileCoordinate coordinate)
  {
    return new LiquidBufferCommitResult(
      LiquidBufferCommitStatus.Duplicate,
      false,
      coordinate,
      LiquidBufferCheckingIntent.None,
      null);
  }

  public static LiquidBufferCommitResult Released(
    TileCoordinate coordinate)
  {
    return new LiquidBufferCommitResult(
      LiquidBufferCommitStatus.Released,
      true,
      coordinate,
      LiquidBufferCheckingIntent.Clear(coordinate),
      null);
  }

  public static LiquidBufferCommitResult Reset(bool changed)
  {
    return new LiquidBufferCommitResult(
      LiquidBufferCommitStatus.Reset,
      changed,
      null,
      LiquidBufferCheckingIntent.None,
      null);
  }

  public static LiquidBufferCommitResult Rejected(string reason)
  {
    return new LiquidBufferCommitResult(
      LiquidBufferCommitStatus.Rejected,
      false,
      null,
      LiquidBufferCheckingIntent.None,
      reason);
  }
}
