namespace NSSLC.NetworkVerification;

internal static class Verify {
  public static void That(bool condition, string message) {
    if (!condition) {
      throw new InvalidOperationException(message);
    }
  }

  public static TException Throws<TException>(Action action) where TException : Exception {
    try {
      action();
    } catch (TException exception) {
      return exception;
    }
    throw new InvalidOperationException($"Expected {typeof(TException).Name}.");
  }

  public static async Task<TException> ThrowsAsync<TException>(Func<Task> action)
      where TException : Exception {
    try {
      await action();
    } catch (TException exception) {
      return exception;
    }
    throw new InvalidOperationException($"Expected {typeof(TException).Name}.");
  }

  public static async Task EventuallyAsync(Func<bool> condition, string message) {
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    while (!condition()) {
      try {
        await Task.Delay(5, timeout.Token);
      } catch (OperationCanceledException) {
        throw new InvalidOperationException(message);
      }
    }
  }
}
