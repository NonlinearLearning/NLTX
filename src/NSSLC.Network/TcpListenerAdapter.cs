using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

using Terraria.Communication;

namespace Terraria.Network;

/// <summary>Owns a TCP listener and the accepted sockets for each callback lifetime.</summary>
public sealed class TcpListenerAdapter : IAsyncDisposable
{
  private readonly object _gate = new();
  private readonly TcpListener _listener;
  private readonly TcpListenerAdapterOptions _options;
  private readonly SemaphoreSlim _connectionSlots;
  private readonly CancellationTokenSource _acceptStop = new();
  private readonly CancellationTokenSource _stop = new();
  private readonly TaskCompletionSource<bool> _acceptLoopCompleted =
    new(TaskCreationOptions.RunContinuationsAsynchronously);
  private readonly HashSet<ConnectionWorker> _workers = [];
  private readonly ConcurrentQueue<Exception> _failureCallbackErrors = new();
  private Task? _runTask;
  private Task? _stopAcceptingTask;
  private Task? _stopTask;
  private Task? _disposeTask;
  private bool _stopRequested;
  private bool _disposed;

  public TcpListenerAdapter(IPEndPoint endpoint, TcpListenerAdapterOptions? options = null)
  {
    ArgumentNullException.ThrowIfNull(endpoint);
    _options = options ?? new TcpListenerAdapterOptions();
    _options.Validate();
    _listener = new TcpListener(endpoint);
    _connectionSlots = new SemaphoreSlim(
      _options.MaximumConcurrentConnections,
      _options.MaximumConcurrentConnections);
  }

  public bool IsRunning
  {
    get
    {
      lock (_gate)
      {
        return _runTask is { IsCompleted: false };
      }
    }
  }

  public Task RunAsync(
    Func<Socket, CancellationToken, Task> onConnection,
    Func<Exception, ValueTask> onConnectionFailure,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(onConnection);
    ArgumentNullException.ThrowIfNull(onConnectionFailure);
    lock (_gate)
    {
      ObjectDisposedException.ThrowIf(_disposed, this);
      if (_stopRequested)
      {
        throw new InvalidOperationException("A stopped TCP listener cannot be restarted.");
      }

      if (_runTask is not null)
      {
        throw new InvalidOperationException("A TCP listener can only be run once.");
      }

      _runTask = RunCoreAsync(onConnection, onConnectionFailure, cancellationToken);
      return _runTask;
    }
  }

  /// <summary>
  /// Closes the listener and waits for the accept loop to exit. Connections admitted before
  /// this request keep running with their original token until full shutdown or caller
  /// cancellation. The returned task is the acceptance barrier only; the caller must await
  /// the full RunAsync task or StopAsync before disposing worker-owned resources.
  /// </summary>
  public Task StopAcceptingAsync()
  {
    lock (_gate)
    {
      if (_stopAcceptingTask is null)
      {
        _stopRequested = true;
        _stopAcceptingTask = StopAcceptingCoreAsync(_runTask is not null);
      }

      return _stopAcceptingTask;
    }
  }

  /// <summary>
  /// Cancels acceptance and workers, then waits for the accept loop and every transferred worker.
  /// Repeated callers share the same completion task.
  /// </summary>
  public Task StopAsync()
  {
    lock (_gate)
    {
      if (_stopTask is null)
      {
        _stopRequested = true;
        _stopTask = StopCoreAsync(_runTask);
      }

      return _stopTask;
    }
  }

  private async Task StopCoreAsync(Task? runTask)
  {
    await Task.Yield();
    Task stopAccepting = StopAcceptingAsync();
    try
    {
      if (!_stop.IsCancellationRequested)
      {
        _stop.Cancel();
      }
    }
    finally
    {
      // Cancellation callbacks may throw; worker resources still belong to RunAsync.
      try
      {
        await stopAccepting.ConfigureAwait(false);
      }
      finally
      {
        if (runTask is not null)
        {
          await runTask.ConfigureAwait(false);
        }
      }
    }
  }

  private async Task StopAcceptingCoreAsync(bool runStarted)
  {
    await Task.Yield();
    try
    {
      _acceptStop.Cancel();
    }
    finally
    {
      StopListener();
      if (runStarted)
      {
        await _acceptLoopCompleted.Task.ConfigureAwait(false);
      }
    }
  }

  /// <summary>
  /// Completes after StopAsync and worker cleanup. Every caller receives the same completion;
  /// host wrappers must preserve that convergence instead of returning on a later call merely
  /// because the disposed flag is already set.
  /// </summary>
  public ValueTask DisposeAsync()
  {
    lock (_gate)
    {
      if (_disposeTask is null)
      {
        _disposed = true;
        _stopRequested = true;
        _disposeTask = DisposeCoreAsync();
      }

      return new ValueTask(_disposeTask);
    }
  }

  private async Task DisposeCoreAsync()
  {
    await Task.Yield();
    try
    {
      await StopAsync().ConfigureAwait(false);
    }
    finally
    {
      _connectionSlots.Dispose();
      _acceptStop.Dispose();
      _stop.Dispose();
    }
  }

  private async Task RunCoreAsync(
    Func<Socket, CancellationToken, Task> onConnection,
    Func<Exception, ValueTask> onConnectionFailure,
    CancellationToken cancellationToken)
  {
    using CancellationTokenSource linkedStop =
      CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _stop.Token);
    CancellationToken runToken = linkedStop.Token;
    using CancellationTokenSource acceptStop =
      CancellationTokenSource.CreateLinkedTokenSource(runToken, _acceptStop.Token);
    try
    {
      _listener.Start(_options.Backlog);
      await AcceptLoopAsync(onConnection, onConnectionFailure, acceptStop.Token, runToken)
        .ConfigureAwait(false);
    }
    catch (OperationCanceledException) when (acceptStop.IsCancellationRequested)
    {
    }
    catch (SocketException) when (acceptStop.IsCancellationRequested)
    {
    }
    catch (ObjectDisposedException) when (acceptStop.IsCancellationRequested)
    {
    }
    catch
    {
      _stop.Cancel();
      throw;
    }
    finally
    {
      StopListener();
      _acceptLoopCompleted.TrySetResult(true);
      await WaitForConnectionsAsync().ConfigureAwait(false);
    }

    Exception[] failureCallbackErrors = _failureCallbackErrors.ToArray();
    if (failureCallbackErrors.Length > 0)
    {
      throw new AggregateException(
        "A TCP connection failure callback failed.",
        failureCallbackErrors);
    }
  }

  private async Task AcceptLoopAsync(
    Func<Socket, CancellationToken, Task> onConnection,
    Func<Exception, ValueTask> onConnectionFailure,
    CancellationToken acceptCancellation,
    CancellationToken connectionCancellation)
  {
    while (!acceptCancellation.IsCancellationRequested)
    {
      await _connectionSlots.WaitAsync(acceptCancellation).ConfigureAwait(false);
      Socket? socket = null;
      bool transferredToWorker = false;
      try
      {
        ValueTask<Socket> acceptOperation;
        lock (_gate)
        {
          // StopListener uses the same gate; do not begin accepting on a stopped listener.
          if (_stopRequested || acceptCancellation.IsCancellationRequested)
          {
            return;
          }

          acceptOperation = _listener.AcceptSocketAsync(acceptCancellation);
        }

        socket = await acceptOperation.ConfigureAwait(false);
        socket.NoDelay = _options.NoDelay;
        var worker = new ConnectionWorker(
          socket,
          onConnection,
          onConnectionFailure,
          _connectionSlots,
          connectionCancellation,
          RemoveWorker,
          RecordFailureCallbackError);
        lock (_gate)
        {
          // Admission and stopping share this gate even if accept already returned a socket.
          if (_stopRequested || acceptCancellation.IsCancellationRequested)
          {
            return;
          }

          _workers.Add(worker);
          worker.Start();
          transferredToWorker = true;
        }
      }
      finally
      {
        if (!transferredToWorker)
        {
          socket?.Dispose();
          _connectionSlots.Release();
        }
      }
    }
  }

  private async Task WaitForConnectionsAsync()
  {
    Task[] completions;
    lock (_gate)
    {
      completions = _workers.Select(static worker => worker.Completion).ToArray();
    }

    await Task.WhenAll(completions).ConfigureAwait(false);
  }

  private void RemoveWorker(ConnectionWorker worker)
  {
    lock (_gate)
    {
      _workers.Remove(worker);
    }
  }

  private void RecordFailureCallbackError(Exception exception)
  {
    _failureCallbackErrors.Enqueue(exception);
    try
    {
      if (!_stop.IsCancellationRequested)
      {
        _stop.Cancel();
      }
    }
    catch (AggregateException cancellationException)
    {
      // The worker removes itself before its task completes, so RunAsync reports these
      // failures from the shared queue rather than relying on a worker task snapshot.
      _failureCallbackErrors.Enqueue(cancellationException);
    }
    finally
    {
      StopListener();
    }
  }

  private void StopListener()
  {
    lock (_gate)
    {
      _listener.Stop();
    }
  }

  private sealed class ConnectionWorker
  {
    private readonly Socket _socket;
    private readonly Func<Socket, CancellationToken, Task> _onConnection;
    private readonly Func<Exception, ValueTask> _onConnectionFailure;
    private readonly SemaphoreSlim _connectionSlots;
    private readonly CancellationToken _cancellationToken;
    private readonly Action<ConnectionWorker> _onCompleted;
    private readonly Action<Exception> _onFailureCallbackError;
    private readonly TaskCompletionSource<bool> _startSignal =
      new(TaskCreationOptions.RunContinuationsAsynchronously);

    public ConnectionWorker(
      Socket socket,
      Func<Socket, CancellationToken, Task> onConnection,
      Func<Exception, ValueTask> onConnectionFailure,
      SemaphoreSlim connectionSlots,
      CancellationToken cancellationToken,
      Action<ConnectionWorker> onCompleted,
      Action<Exception> onFailureCallbackError)
    {
      _socket = socket;
      _onConnection = onConnection;
      _onConnectionFailure = onConnectionFailure;
      _connectionSlots = connectionSlots;
      _cancellationToken = cancellationToken;
      _onCompleted = onCompleted;
      _onFailureCallbackError = onFailureCallbackError;
    }

    public Task Completion { get; private set; } = Task.CompletedTask;

    public void Start()
    {
      Completion = RunAsync();
      _startSignal.TrySetResult(true);
    }

    private async Task RunAsync()
    {
      await _startSignal.Task.ConfigureAwait(false);
      try
      {
        _cancellationToken.ThrowIfCancellationRequested();
        await _onConnection(_socket, _cancellationToken).ConfigureAwait(false);
      }
      catch (OperationCanceledException) when (_cancellationToken.IsCancellationRequested)
      {
      }
      catch (Exception exception)
      {
        try
        {
          await _onConnectionFailure(exception).ConfigureAwait(false);
        }
        catch (Exception failureCallbackException)
        {
          _onFailureCallbackError(failureCallbackException);
        }
      }
      finally
      {
        _socket.Dispose();
        _connectionSlots.Release();
        _onCompleted(this);
      }
    }
  }
}
