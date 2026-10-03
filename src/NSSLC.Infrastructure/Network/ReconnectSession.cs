using Terraria.Network;

namespace NSSLC.Infrastructure.Network;

public sealed class ReconnectSession {
  private readonly object _gate = new();
  private readonly TimeProvider _time;
  private readonly CancellationTokenSource _deadline;
  private readonly Action _activated;
  private long _activeAt;
  private bool _active;
  private bool _finished;
  private long _finishedAt;

  public IPacketConnection Connection { get; }
  internal bool Stable(TimeSpan period) {
    lock (_gate) {
      return _active && _time.GetElapsedTime(_activeAt,
          _finished ? _finishedAt : _time.GetTimestamp()) >= period;
    }
  }

  internal ReconnectSession(IPacketConnection connection, TimeProvider time,
      CancellationTokenSource deadline, Action activated) {
    Connection = connection;
    _time = time;
    _deadline = deadline;
    _activated = activated;
  }

  public void MarkActive() {
    lock (_gate) {
      if (_finished || _deadline.IsCancellationRequested) {
        throw new InvalidOperationException("This connection's rebuild has ended.");
      }
      if (_active) {
        return;
      }
      _active = true;
      _activeAt = _time.GetTimestamp();
      _deadline.CancelAfter(Timeout.InfiniteTimeSpan);
      _activated.Invoke();
    }
  }

  internal void Finish() {
    lock (_gate) {
      _finishedAt = _time.GetTimestamp();
      _finished = true;
    }
  }
}
