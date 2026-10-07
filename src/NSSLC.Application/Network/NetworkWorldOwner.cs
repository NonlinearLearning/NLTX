using System.Collections.Concurrent;
using Terraria.NonAuthoritative.Persistence;
using Terraria.WorldSession.Components;

namespace Terraria.Network;

public readonly record struct NetworkTeamSpawnPoint(int X, int Y);


/// <summary>Runs world commands and entity access on the session's owning thread.</summary>
/// <remarks>
/// The factory and operations are synchronous. Return detached results rather than component
/// borrows or live session state. Cancellation skips queued operations; once execution starts,
/// the caller receives the actual result. Disposal rejects pending work, waits for the running
/// operation, and disposes the session on its owner thread.
/// </remarks>
public sealed class NetworkWorldOwner : IAsyncDisposable {
  private interface IQueuedOperation {
    bool TryStart();
    void Execute(LoadedWorldSession session);
    void Reject(Exception exception);
  }

  private sealed class QueuedOperation<TResult> : IQueuedOperation {
    private readonly Func<LoadedWorldSession, TResult> _operation;
    private readonly CancellationToken _cancellationToken;
    private readonly CancellationTokenRegistration _cancellationRegistration;
    private readonly TaskCompletionSource<TResult> _result =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int _state;

    public Task<TResult> Result => _result.Task;

    public QueuedOperation(Func<LoadedWorldSession, TResult> operation,
        CancellationToken cancellationToken) {
      _operation = operation;
      _cancellationToken = cancellationToken;
      _cancellationRegistration = cancellationToken.Register(CancelWhileQueued);
    }

    public bool TryStart() {
      return Interlocked.CompareExchange(ref _state, 1, 0) == 0;
    }

    public void Execute(LoadedWorldSession session) {
      try {
        _result.TrySetResult(_operation(session));
      } catch (Exception exception) {
        _result.TrySetException(exception);
      } finally {
        _cancellationRegistration.Dispose();
      }
    }

    public void Reject(Exception exception) {
      if (Interlocked.CompareExchange(ref _state, 1, 0) == 0) {
        _result.TrySetException(exception);
      }
      _cancellationRegistration.Dispose();
    }

    private void CancelWhileQueued() {
      if (Interlocked.CompareExchange(ref _state, 2, 0) == 0) {
        _result.TrySetCanceled(_cancellationToken);
      }
    }
  }

  private readonly BlockingCollection<IQueuedOperation> _commands;
  private readonly object _lifecycleGate = new();
  private readonly Func<LoadedWorldSession> _sessionFactory;
  private readonly Thread _thread;
  private readonly TaskCompletionSource _ready =
      new(TaskCreationOptions.RunContinuationsAsynchronously);
  private readonly TaskCompletionSource _completion =
      new(TaskCreationOptions.RunContinuationsAsynchronously);
  private int _stopping;
  private int _disposeStarted;

  public Task Ready => _ready.Task;

  public NetworkWorldOwner(Func<LoadedWorldSession> sessionFactory,
      int maximumPendingCommands = 1024) {
    ArgumentNullException.ThrowIfNull(sessionFactory);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumPendingCommands);
    _sessionFactory = sessionFactory;
    _commands = new BlockingCollection<IQueuedOperation>(maximumPendingCommands);
    _thread = new Thread(Run) {
      IsBackground = true,
      Name = "Network world owner"
    };
    try {
      _thread.Start();
    } catch {
      _commands.Dispose();
      throw;
    }
  }

  public ValueTask<TResult> InvokeAsync<TResult>(Func<LoadedWorldSession, TResult> operation,
      CancellationToken cancellationToken = default) {
    ArgumentNullException.ThrowIfNull(operation);
    cancellationToken.ThrowIfCancellationRequested();
    ObjectDisposedException.ThrowIf(Volatile.Read(ref _stopping) != 0, this);
    if (Environment.CurrentManagedThreadId == _thread.ManagedThreadId) {
      throw new InvalidOperationException("World commands cannot enqueue nested owner work.");
    }
    var pending = new QueuedOperation<TResult>(operation, cancellationToken);
    try {
      if (!_commands.TryAdd(pending)) {
        pending.Reject(new InvalidOperationException("The world command queue is full."));
      }
    } catch (InvalidOperationException) {
      pending.Reject(new ObjectDisposedException(nameof(NetworkWorldOwner)));
    }
    return new ValueTask<TResult>(pending.Result);
  }

  /// <summary>
  /// Reads the authenticated world's persisted team spawn projection on the owner thread.
  /// The result is detached and is null when the world does not enable team-based spawns or
  /// does not contain a valid point for the requested team.
  /// </summary>
  public ValueTask<NetworkTeamSpawnPoint?> FindTeamSpawnPointAsync(
      int team,
      CancellationToken cancellationToken = default) {
    cancellationToken.ThrowIfCancellationRequested();
    return InvokeAsync<NetworkTeamSpawnPoint?>(session => {
      if (team < 0 || team > 5 ||
          (session.World.Rules.SecretSeeds & WorldSecretSeedFlags.TeamBasedSpawns) == 0) {
        return null;
      }

      IReadOnlyList<Terraria.WorldStorage.TileCoordinate> points =
          session.World.Descriptor.ExtraSpawnPoints;
      if (team >= points.Count) {
        return null;
      }

      Terraria.WorldStorage.TileCoordinate point = points[team];
      if (point.X < 0 || point.Y < 0 || point.X >= session.World.Descriptor.SizeX ||
          point.Y >= session.World.Descriptor.SizeY) {
        return null;
      }

      return new NetworkTeamSpawnPoint(point.X, point.Y);
    }, cancellationToken);
  }

  public async ValueTask DisposeAsync() {
    if (Environment.CurrentManagedThreadId == _thread.ManagedThreadId) {
      throw new InvalidOperationException("The world owner cannot dispose itself from a command.");
    }
    bool firstDisposal = Interlocked.Exchange(ref _disposeStarted, 1) == 0;
    if (firstDisposal) {
      lock (_lifecycleGate) {
        Interlocked.Exchange(ref _stopping, 1);
      }
      _commands.CompleteAdding();
    }
    try {
      await _completion.Task.ConfigureAwait(false);
    } finally {
      if (firstDisposal) {
        _commands.Dispose();
      }
    }
  }

  private void Run() {
    LoadedWorldSession? session = null;
    Exception? failure = null;
    try {
      session = _sessionFactory()
          ?? throw new InvalidOperationException("The world session factory returned null.");
      _ready.TrySetResult();
      foreach (IQueuedOperation operation in _commands.GetConsumingEnumerable()) {
        bool execute;
        lock (_lifecycleGate) {
          execute = _stopping == 0 && operation.TryStart();
        }
        if (execute) {
          operation.Execute(session);
        } else {
          operation.Reject(new ObjectDisposedException(nameof(NetworkWorldOwner)));
        }
      }
    } catch (Exception exception) {
      failure = exception;
      _ready.TrySetException(exception);
    } finally {
      lock (_lifecycleGate) {
        Interlocked.Exchange(ref _stopping, 1);
      }
      _commands.CompleteAdding();
      while (_commands.TryTake(out IQueuedOperation? pending)) {
        pending.Reject(failure ?? new ObjectDisposedException(nameof(NetworkWorldOwner)));
      }
      try {
        session?.Dispose();
      } catch (Exception exception) {
        failure = failure is null ? exception : new AggregateException(failure, exception);
      }
      if (failure is null) {
        _completion.TrySetResult();
      } else {
        _completion.TrySetException(failure);
      }
    }
  }
}
