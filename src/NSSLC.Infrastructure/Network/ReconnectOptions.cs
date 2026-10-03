namespace NSSLC.Infrastructure.Network;

public sealed record ReconnectOptions {
  public int MaximumAttempts { get; init; } = 8;
  public TimeSpan TotalBudget { get; init; } = TimeSpan.FromSeconds(60);
  public TimeSpan ConnectTimeout { get; init; } = TimeSpan.FromSeconds(5);
  public TimeSpan InitialDelay { get; init; } = TimeSpan.FromMilliseconds(200);
  public TimeSpan MaximumDelay { get; init; } = TimeSpan.FromSeconds(10);
  public TimeSpan StableActivePeriod { get; init; } = TimeSpan.FromSeconds(30);
  public TimeSpan CleanupTimeout { get; init; } = TimeSpan.FromSeconds(5);

  internal void Validate() {
    if (MaximumAttempts is < 1 or > 8 || TotalBudget <= TimeSpan.Zero
        || TotalBudget > TimeSpan.FromSeconds(60) || ConnectTimeout <= TimeSpan.Zero
        || ConnectTimeout > TimeSpan.FromSeconds(5) || InitialDelay < TimeSpan.Zero
        || MaximumDelay < InitialDelay || MaximumDelay > TimeSpan.FromSeconds(10)
        || StableActivePeriod <= TimeSpan.Zero || CleanupTimeout <= TimeSpan.Zero) {
      throw new ArgumentOutOfRangeException(nameof(ReconnectOptions));
    }
  }
}
