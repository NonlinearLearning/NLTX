namespace NSSLC.NetworkVerification;

internal sealed class ManualTimeProvider : TimeProvider {
  private sealed class ManualTimer : ITimer {
    private readonly ManualTimeProvider _owner;
    private readonly TimerCallback _callback;
    private readonly object? _state;
    private TimeSpan _due;
    private TimeSpan _period;
    private bool _disposed;

    public ManualTimer(ManualTimeProvider owner, TimerCallback callback, object? state,
        TimeSpan due, TimeSpan period) {
      _owner = owner;
      _callback = callback;
      _state = state;
      Change(due, period);
    }

    public bool Change(TimeSpan dueTime, TimeSpan period) {
      if (_disposed) {
        return false;
      }
      _due = dueTime == Timeout.InfiniteTimeSpan ? TimeSpan.MaxValue : _owner._elapsed + dueTime;
      _period = period;
      return true;
    }

    public void Dispose() {
      _disposed = true;
    }

    public ValueTask DisposeAsync() {
      Dispose();
      return ValueTask.CompletedTask;
    }

    public void Fire() {
      while (!_disposed && _due <= _owner._elapsed) {
        _due = _period <= TimeSpan.Zero ? TimeSpan.MaxValue : _due + _period;
        _callback.Invoke(_state);
      }
    }

    public TimeSpan? Remaining {
      get {
        return _disposed || _due == TimeSpan.MaxValue ? null : _due - _owner._elapsed;
      }
    }
  }

  private readonly List<ManualTimer> _timers = new();
  private readonly DateTimeOffset _origin = new(2026, 10, 3, 0, 0, 0, TimeSpan.Zero);
  private TimeSpan _elapsed;

  public override long TimestampFrequency => TimeSpan.TicksPerSecond;
  public TimeSpan? NextTimerDelay {
    get {
      TimeSpan? next = null;
      foreach (ManualTimer timer in _timers.ToArray()) {
        if (timer.Remaining is TimeSpan remaining && (next is null || remaining < next)) {
          next = remaining;
        }
      }
      return next;
    }
  }

  public override DateTimeOffset GetUtcNow() {
    return _origin + _elapsed;
  }

  public override long GetTimestamp() {
    return _elapsed.Ticks;
  }

  public override ITimer CreateTimer(TimerCallback callback, object? state,
      TimeSpan dueTime, TimeSpan period) {
    var timer = new ManualTimer(this, callback, state, dueTime, period);
    _timers.Add(timer);
    return timer;
  }

  public void Advance(TimeSpan duration) {
    _elapsed += duration;
    foreach (ManualTimer timer in _timers.ToArray()) {
      timer.Fire();
    }
  }
}
