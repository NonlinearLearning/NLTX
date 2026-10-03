using System.Net.Sockets;
using Terraria.Network;

namespace NSSLC.Infrastructure.Network;

public sealed class ReconnectCoordinator : IAsyncDisposable {
  private readonly object _gate = new();
  private readonly Func<TimeSpan, CancellationToken, ValueTask<IPacketConnection>> _connect;
  private readonly ReconnectOptions _options;
  private readonly TimeProvider _time;
  private readonly Func<double> _random;
  private readonly CancellationTokenSource _stopping = new();
  private Task<ReconnectRunResult>? _run;
  private int _state = (int)ReconnectState.Stopped;
  private long _attempts;

  public ReconnectState State => (ReconnectState)Volatile.Read(ref _state);
  public long Attempts => Interlocked.Read(ref _attempts);

  public ReconnectCoordinator(
      Func<TimeSpan, CancellationToken, ValueTask<IPacketConnection>> connect,
      ReconnectOptions? options = null, TimeProvider? timeProvider = null,
      Func<double>? random = null) {
    ArgumentNullException.ThrowIfNull(connect);
    _connect = connect;
    _options = options ?? new();
    _options.Validate();
    _time = timeProvider ?? TimeProvider.System;
    _random = random ?? Random.Shared.NextDouble;
  }

  public Task<ReconnectRunResult> RunAsync(
      Func<ReconnectSession, CancellationToken, Task<ReconnectSessionOutcome>> rebuildAndUse,
      CancellationToken cancellationToken = default) {
    ArgumentNullException.ThrowIfNull(rebuildAndUse);
    lock (_gate) {
      if (_run is not null || _stopping.IsCancellationRequested) {
        throw new InvalidOperationException("A reconnect coordinator runs once and cannot restart.");
      }
      _run = RunCoreAsync(rebuildAndUse, cancellationToken);
      return _run;
    }
  }

  public void Stop() {
    _stopping.Cancel();
  }

  public async ValueTask DisposeAsync() {
    Stop();
    Task<ReconnectRunResult>? run;
    lock (_gate) { run = _run; }
    if (run is not null) {
      await run.ConfigureAwait(false);
    }
  }

  private async Task<ReconnectRunResult> RunCoreAsync(
      Func<ReconnectSession, CancellationToken, Task<ReconnectSessionOutcome>> use,
      CancellationToken cancellationToken) {
    using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(
        cancellationToken, _stopping.Token);
    long budgetStarted = _time.GetTimestamp();
    int used = 0;
    Exception? lastError = null;
    try {
      while (used < _options.MaximumAttempts) {
        lifetime.Token.ThrowIfCancellationRequested();
        TimeSpan remaining = _options.TotalBudget - _time.GetElapsedTime(budgetStarted);
        if (remaining <= TimeSpan.Zero) {
          return new(Attempts, "BudgetExhausted", lastError);
        }
        used++;
        Interlocked.Increment(ref _attempts);
        SetState(ReconnectState.Connecting);
        TimeSpan connectTime = remaining < _options.ConnectTimeout
            ? remaining : _options.ConnectTimeout;
        IPacketConnection? connection = null;
        ReconnectSession? session = null;
        using var failureDeadline = new CancellationTokenSource(remaining, _time);
        using var operation = CancellationTokenSource.CreateLinkedTokenSource(
            lifetime.Token, failureDeadline.Token);
        try {
          using var connectDeadline = new CancellationTokenSource(connectTime, _time);
          using var connecting = CancellationTokenSource.CreateLinkedTokenSource(
              operation.Token, connectDeadline.Token);
          Task<IPacketConnection> pending = _connect.Invoke(connectTime, connecting.Token).AsTask();
          try {
            connection = await pending.WaitAsync(connecting.Token).ConfigureAwait(false);
          } catch (OperationCanceledException) when (connecting.IsCancellationRequested) {
            _ = DisposeLateConnectionAsync(pending);
            if (connectDeadline.IsCancellationRequested && !operation.IsCancellationRequested) {
              throw new TimeoutException("Connection attempt timed out.");
            }
            throw;
          }
          operation.Token.ThrowIfCancellationRequested();
          SetState(ReconnectState.Handshaking);
          session = new(connection, _time, failureDeadline, () => SetState(ReconnectState.Active));
          ReconnectSessionOutcome outcome = await use.Invoke(session, operation.Token)
              .WaitAsync(operation.Token).ConfigureAwait(false);
          if (outcome == ReconnectSessionOutcome.Stop) {
            return new(Attempts, "SessionStopped");
          }
          if (outcome != ReconnectSessionOutcome.RetryAfterDisconnect) {
            return new(Attempts, "InvalidSessionOutcome");
          }
          lastError = null;
        } catch (OperationCanceledException) when (lifetime.IsCancellationRequested) {
          return new(Attempts, "Stopped");
        } catch (OperationCanceledException) when (operation.IsCancellationRequested) {
          lastError = new TimeoutException("Connection rebuild exceeded its failure budget.");
        } catch (Exception error) when (CanRetry(error)) {
          lastError = error;
        } catch (Exception error) {
          return new(Attempts, "TerminalFailure", error);
        } finally {
          if (session is not null) {
            session.Finish();
          }
          if (connection is not null) {
            await DisposeConnectionAsync(connection).ConfigureAwait(false);
          }
        }
        if (session is not null && session.Stable(_options.StableActivePeriod)) {
          used = 0;
          budgetStarted = _time.GetTimestamp();
        }
        lifetime.Token.ThrowIfCancellationRequested();
        if (used >= _options.MaximumAttempts) {
          break;
        }
        remaining = _options.TotalBudget - _time.GetElapsedTime(budgetStarted);
        if (remaining <= TimeSpan.Zero) {
          return new(Attempts, "BudgetExhausted", lastError);
        }
        double sample = _random.Invoke();
        if (!double.IsFinite(sample) || sample < 0 || sample >= 1) {
          return new(Attempts, "InvalidRandomSample");
        }
        double cap = Math.Min(_options.MaximumDelay.TotalMilliseconds,
            _options.InitialDelay.TotalMilliseconds * Math.Pow(2, Math.Max(0, used - 1)));
        TimeSpan delay = TimeSpan.FromMilliseconds(cap * sample);
        if (delay > remaining) {
          delay = remaining;
        }
        SetState(ReconnectState.Backoff);
        await Task.Delay(delay, _time, lifetime.Token).ConfigureAwait(false);
      }
      return new(Attempts, "AttemptsExhausted", lastError);
    } catch (OperationCanceledException) when (lifetime.IsCancellationRequested) {
      return new(Attempts, "Stopped");
    } catch (Exception error) {
      return new(Attempts, "CoordinatorFailure", error);
    } finally {
      SetState(ReconnectState.Stopped);
    }
  }

  private static bool CanRetry(Exception error) {
    return error is SocketException or TimeoutException
        || (error is IOException && error is not PacketProtocolException);
  }

  private async Task DisposeConnectionAsync(IPacketConnection connection) {
    using var cleanup = new CancellationTokenSource(_options.CleanupTimeout, _time);
    await connection.DisposeAsync().AsTask().WaitAsync(cleanup.Token).ConfigureAwait(false);
  }

  private async Task DisposeLateConnectionAsync(Task<IPacketConnection> pending) {
    try {
      IPacketConnection connection = await pending.ConfigureAwait(false);
      await DisposeConnectionAsync(connection).ConfigureAwait(false);
    } catch (Exception error) {
      // A late attempt is isolated from RunCore; retain failure for the caller's diagnostics.
      LateCleanupError = error;
    }
  }

  public Exception? LateCleanupError { get; private set; }

  private void SetState(ReconnectState state) {
    Volatile.Write(ref _state, (int)state);
  }
}
