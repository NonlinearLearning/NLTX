namespace Terraria.Network;

/// <summary>Advances authoritative emote-bubble lifetimes at the game's 60 Hz tick rate.</summary>
public static class SocialEmoteBubbleTickScheduler {
  private static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(1.0 / 60.0);

  /// <summary>
  /// Runs one owner tick for each timer signal until canceled. Cancellation completes only after
  /// any in-progress synchronous owner tick has returned.
  /// </summary>
  public static async Task RunAsync(SocialEmoteBubbleStateOwner owner, TimeProvider timeProvider,
      CancellationToken cancellationToken) {
    ArgumentNullException.ThrowIfNull(owner);
    ArgumentNullException.ThrowIfNull(timeProvider);

    using var timer = new PeriodicTimer(TickInterval, timeProvider);
    try {
      while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false)) {
        owner.AdvanceTick();
      }
    } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
      // Host shutdown waits for this task before disposing the owner.
    }
  }
}
