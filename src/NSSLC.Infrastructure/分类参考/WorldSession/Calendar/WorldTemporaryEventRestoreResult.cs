namespace Terraria.NonAuthoritative.WorldSession.Calendar;

public readonly record struct WorldTemporaryEventRestoreResult(
  bool Applied,
  string? FailureCode)
{
  public static WorldTemporaryEventRestoreResult Success => new(true, null);

  public static WorldTemporaryEventRestoreResult Failed(string failureCode)
  {
    return new(
      false,
      string.IsNullOrWhiteSpace(failureCode)
        ? throw new ArgumentException("A failure code is required.", nameof(failureCode))
        : failureCode);
  }
}
