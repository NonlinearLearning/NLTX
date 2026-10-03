namespace NSSLC.Infrastructure.Network;

public sealed class PacketByteBudget {
  private readonly object _gate = new();
  private TaskCompletionSource _changed = NewSignal();
  private int _used;
  internal SemaphoreSlim LargeCodecGate { get; } = new(1, 1);

  public int Limit { get; }
  public int Used {
    get {
      lock (_gate) {
        return _used;
      }
    }
  }

  public PacketByteBudget(int limit = 64 * 1024 * 1024) {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);
    Limit = limit;
  }

  public bool TryReserve(int bytes) {
    ArgumentOutOfRangeException.ThrowIfNegative(bytes);
    lock (_gate) {
      if (bytes > Limit - _used) {
        return false;
      }
      _used += bytes;
      return true;
    }
  }

  internal async ValueTask ReserveAsync(int bytes, CancellationToken cancellationToken) {
    if (bytes > Limit) {
      throw new ArgumentOutOfRangeException(nameof(bytes));
    }
    while (true) {
      Task changed;
      lock (_gate) {
        cancellationToken.ThrowIfCancellationRequested();
        if (bytes <= Limit - _used) {
          _used += bytes;
          return;
        }
        changed = _changed.Task;
      }
      await changed.WaitAsync(cancellationToken).ConfigureAwait(false);
    }
  }

  public void Release(int bytes) {
    lock (_gate) {
      if (bytes < 0 || bytes > _used) {
        throw new InvalidOperationException("Unbalanced packet byte reservation.");
      }
      _used -= bytes;
      TaskCompletionSource old = _changed;
      _changed = NewSignal();
      old.TrySetResult();
    }
  }

  private static TaskCompletionSource NewSignal() {
    return new(TaskCreationOptions.RunContinuationsAsynchronously);
  }
}
