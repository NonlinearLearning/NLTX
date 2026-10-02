namespace Terraria.WorldSession.Runtime;

public readonly record struct WorldPersistenceResult(bool Accepted, string? FailureCode)
{
  public static WorldPersistenceResult Success => new(true, null);

  public static WorldPersistenceResult Rejected(string failureCode)
  {
    return new(
      false,
      string.IsNullOrWhiteSpace(failureCode)
        ? throw new ArgumentException("A failure code is required.", nameof(failureCode))
        : failureCode);
  }
}
