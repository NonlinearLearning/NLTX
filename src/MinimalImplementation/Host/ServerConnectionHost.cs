using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Protocol;

namespace MinimalImplementation.Server.Host;

/// <summary>
/// Owns the production listener and the lifetime of each Version4 connection.
/// Domain admission and packet ownership remain explicit callbacks supplied by the composition root.
/// </summary>
public sealed class ServerConnectionHost : IAsyncDisposable
{
  internal sealed class OutboundTurn(
      ServerConnectionHost host, SessionEpoch[] targets, Task ready) : IDisposable {
    private int _disposed;

    internal IReadOnlyList<SessionEpoch> Targets { get; } = targets;
    internal Task Ready { get; } = ready;
    internal TaskCompletionSource Completion { get; } =
      new(TaskCreationOptions.RunContinuationsAsynchronously);

    public void Dispose() {
      if (Interlocked.Exchange(ref _disposed, 1) != 0) {
        return;
      }

      if (Ready.IsCompleted) {
        host.ReleaseOutboundTurn(this);
      } else {
        // A queue rejection can dispose before its predecessors end. Keep their dependency
        // until then so a successor cannot overtake them through this abandoned turn.
        _ = ReleaseAfterPredecessorsAsync();
      }
    }

    private async Task ReleaseAfterPredecessorsAsync() {
      // Ready contains only successful completion tasks from earlier turns; it cannot fault
      // or be cancelled. This continuation performs no owner mutation or transport I/O.
      await Ready.ConfigureAwait(false);
      host.ReleaseOutboundTurn(this);
    }
  }

  private readonly object _gate = new();
  private readonly object _outboundOrderGate = new();
  private readonly TcpListenerAdapter _listener;
  private readonly Version4WireProfile _wireProfile;
  private readonly Func<SessionEpoch> _allocateEpoch;
  private readonly Func<InboundRawFrame, InboundPacketEnvelope> _createEnvelope;
  private readonly Func<InboundVersion4Packet, CancellationToken, ValueTask> _onPacket;
  private readonly Func<ConnectionClosed, ValueTask> _onClosed;
  private readonly Func<Exception, ValueTask> _onConnectionFailure;
  private readonly PacketTcpConnectionOptions? _connectionOptions;
  private readonly TimeProvider? _timeProvider;
  private readonly bool _enableDebugCommands;
  private readonly Func<SessionEpoch, ValueTask>? _onConnected;
  private readonly Dictionary<SessionEpoch, Version4PacketConnection> _connections = [];
  private readonly Dictionary<SessionEpoch, Task> _outboundTails = [];
  private readonly TaskCompletionSource<bool> _packetsDrained =
    new(TaskCreationOptions.RunContinuationsAsynchronously);
  private Task? _runTask;
  private Task? _disposeTask;
  private int _acceptedPacketCount;
  private bool _acceptingStopped;
  private bool _packetAdmissionStopped;
  private bool _stopRequested;
  private bool _disposed;

  public ServerConnectionHost(
    IPEndPoint endpoint,
    Version4WireProfile wireProfile,
    Func<SessionEpoch> allocateEpoch,
    Func<InboundRawFrame, InboundPacketEnvelope> createEnvelope,
    Func<InboundVersion4Packet, CancellationToken, ValueTask> onPacket,
    Func<ConnectionClosed, ValueTask> onClosed,
    Func<Exception, ValueTask> onConnectionFailure,
    TcpListenerAdapterOptions? listenerOptions = null,
    PacketTcpConnectionOptions? connectionOptions = null,
    TimeProvider? timeProvider = null,
    bool enableDebugCommands = false,
    Func<SessionEpoch, ValueTask>? onConnected = null)
  {
    ArgumentNullException.ThrowIfNull(endpoint);
    ArgumentNullException.ThrowIfNull(wireProfile);
    ArgumentNullException.ThrowIfNull(allocateEpoch);
    ArgumentNullException.ThrowIfNull(createEnvelope);
    ArgumentNullException.ThrowIfNull(onPacket);
    ArgumentNullException.ThrowIfNull(onClosed);
    ArgumentNullException.ThrowIfNull(onConnectionFailure);
    _listener = new TcpListenerAdapter(endpoint, listenerOptions);
    _wireProfile = wireProfile;
    _allocateEpoch = allocateEpoch;
    _createEnvelope = createEnvelope;
    _onPacket = onPacket;
    _onClosed = onClosed;
    _onConnectionFailure = onConnectionFailure;
    _connectionOptions = connectionOptions;
    _timeProvider = timeProvider;
    _enableDebugCommands = enableDebugCommands;
    _onConnected = onConnected;
  }

  public bool IsRunning => _listener.IsRunning;

  public int ActiveConnectionCount
  {
    get
    {
      lock (_gate)
      {
        return _connections.Count;
      }
    }
  }

  public Task Completion
  {
    get
    {
      lock (_gate)
      {
        return _runTask ?? Task.CompletedTask;
      }
    }
  }

  public async ValueTask<IReadOnlyList<SendReceipt>> SendAsync(
    OutboundPacketIntent intent,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(intent);
    var targets = new List<Version4PacketConnection>();
    lock (_gate)
    {
      foreach (SessionEpoch epoch in intent.TargetEpochs)
      {
        if (intent.IgnoreEpoch == epoch)
        {
          continue;
        }

        if (_connections.TryGetValue(epoch, out Version4PacketConnection? connection))
        {
          targets.Add(connection);
        }
      }
    }

    var receipts = new List<SendReceipt>(targets.Count);
    foreach (Version4PacketConnection connection in targets)
    {
      receipts.Add(await connection.SendAsync(intent, cancellationToken).ConfigureAwait(false));
    }

    return receipts.AsReadOnly();
  }

  /// <summary>Requests transport shutdown without waiting for the connection run task.</summary>
  public ValueTask CloseAsync(SessionEpoch epoch)
  {
    Version4PacketConnection? connection;
    lock (_gate)
    {
      _connections.TryGetValue(epoch, out connection);
    }

    connection?.RequestClose("LocalRequest");
    return ValueTask.CompletedTask;
  }

  public Task StartAsync(CancellationToken cancellationToken = default)
  {
    lock (_gate)
    {
      ObjectDisposedException.ThrowIf(_disposed, this);
      if (_stopRequested || _acceptingStopped)
      {
        throw new InvalidOperationException("A stopped connection host cannot be restarted.");
      }

      if (_runTask is not null)
      {
        throw new InvalidOperationException("The connection host can only be started once.");
      }

      _runTask = _listener.RunAsync(
        HandleConnectionAsync,
        HandleConnectionFailureAsync,
        cancellationToken);
    }

    return Task.CompletedTask;
  }

  /// <summary>Stops new accepts while already transferred workers retain their connections.</summary>
  public Task StopAcceptingAsync()
  {
    lock (_gate)
    {
      _acceptingStopped = true;
    }

    return _listener.StopAcceptingAsync();
  }

  /// <summary>Closes packet admission without canceling already admitted dispatches or sends.</summary>
  public void StopPacketAdmission()
  {
    lock (_gate)
    {
      _packetAdmissionStopped = true;
      if (_acceptedPacketCount == 0)
      {
        _packetsDrained.TrySetResult(true);
      }
    }
  }

  public Task DrainAcceptedPacketsAsync(CancellationToken cancellationToken = default)
  {
    StopPacketAdmission();
    return _packetsDrained.Task.WaitAsync(cancellationToken);
  }

  public async Task StopAsync()
  {
    Task? runTask;
    lock (_gate)
    {
      _stopRequested = true;
      runTask = _runTask;
    }

    await _listener.StopAsync().ConfigureAwait(false);
    if (runTask is not null && !ReferenceEquals(runTask, Completion))
    {
      await runTask.ConfigureAwait(false);
    }
  }

  public ValueTask DisposeAsync()
  {
    TaskCompletionSource<bool>? disposeCompletion = null;
    Task disposeTask;
    lock (_gate)
    {
      if (_disposeTask is null)
      {
        _disposed = true;
        disposeCompletion = new TaskCompletionSource<bool>(
          TaskCreationOptions.RunContinuationsAsynchronously);
        _disposeTask = disposeCompletion.Task;
      }

      disposeTask = _disposeTask;
    }

    if (disposeCompletion is not null)
    {
      _ = DisposeListenerAsync(disposeCompletion);
    }

    return new ValueTask(disposeTask);
  }

  // Router and periodic delivery batches reserve on the same commit loop. Waiting and sends
  // stay outside it; receipt settlements reenter it. Numeric versions from separate owners
  // do not define their relative order. Direct SendAsync callers must join explicitly.
  internal OutboundTurn? ReserveOutboundTurn(
      ServerLoop commitLoop, SessionEpoch? sectionTarget,
      params IReadOnlyList<OutboundPacketIntent>[] batches) {
    if (!commitLoop.IsCurrent) {
      throw new InvalidOperationException("Outbound turns must be reserved on the server loop.");
    }

    var targets = new HashSet<SessionEpoch>();
    foreach (IReadOnlyList<OutboundPacketIntent> batch in batches) {
      foreach (OutboundPacketIntent intent in batch) {
        foreach (SessionEpoch target in intent.TargetEpochs) {
          if (target != intent.IgnoreEpoch) {
            targets.Add(target);
          }
        }
      }
    }

    if (sectionTarget is SessionEpoch epoch) {
      targets.Add(epoch);
    }

    if (targets.Count == 0) {
      return null;
    }

    lock (_outboundOrderGate) {
      var predecessors = new HashSet<Task>();
      foreach (SessionEpoch target in targets) {
        if (_outboundTails.TryGetValue(target, out Task? tail)) {
          predecessors.Add(tail);
        }
      }

      var turn = new OutboundTurn(this, targets.ToArray(), Task.WhenAll(predecessors));
      foreach (SessionEpoch target in targets) {
        _outboundTails[target] = turn.Completion.Task;
      }

      return turn;
    }
  }

  private void ReleaseOutboundTurn(OutboundTurn turn) {
    lock (_outboundOrderGate) {
      foreach (SessionEpoch target in turn.Targets) {
        if (_outboundTails.TryGetValue(target, out Task? tail) &&
            ReferenceEquals(tail, turn.Completion.Task)) {
          _outboundTails.Remove(target);
        }
      }
    }

    // Completion means the attempt ended, including rejection/failure, not delivery ack.
    turn.Completion.TrySetResult();
  }

  private async Task DisposeListenerAsync(TaskCompletionSource<bool> completion)
  {
    try
    {
      await _listener.DisposeAsync().ConfigureAwait(false);
      completion.TrySetResult(true);
    }
    catch (Exception exception)
    {
      completion.TrySetException(exception);
    }
  }

  private async Task HandleConnectionAsync(Socket socket, CancellationToken cancellationToken)
  {
    SessionEpoch epoch = _allocateEpoch();
    if (!epoch.IsValid)
    {
      throw new InvalidOperationException("The connection epoch allocator returned an invalid epoch.");
    }

    Version4PacketConnection? connection = null;
    bool connectionRegistered = false;
    bool admissionCompleted = _onConnected is null;
    try
    {
      connection = new Version4PacketConnection(
        socket,
        epoch,
        _wireProfile,
        _createEnvelope,
        DispatchPacketAsync,
        _enableDebugCommands,
        _connectionOptions,
        _timeProvider,
        closed => HandleClosedAsync(epoch, closed));
      lock (_gate)
      {
        if (_stopRequested || _disposed)
        {
          throw new InvalidOperationException("A new connection arrived after host shutdown began.");
        }

        if (!_connections.TryAdd(epoch, connection))
        {
          throw new InvalidOperationException($"Session epoch {epoch.Value} was allocated twice.");
        }

        connectionRegistered = true;
      }

      string remoteEndpoint = socket.RemoteEndPoint?.ToString() ?? "unknown";
      ServerLog.Info(
        "server.connection.accepted",
        $"epoch={epoch.Value} remote={ServerLog.Quote(remoteEndpoint)}");

      if (_onConnected is not null)
      {
        await _onConnected(epoch).ConfigureAwait(false);
        admissionCompleted = true;
        ServerLog.Info("server.connection.session_opened", $"epoch={epoch.Value}");
      }

      await connection.RunAsync(cancellationToken).ConfigureAwait(false);
    }
    finally
    {
      if (connection is not null)
      {
        try
        {
          if (connectionRegistered && !admissionCompleted)
          {
            await _onClosed(
              new ConnectionClosed(
                epoch,
                "AdmissionFailure",
                new PacketSequence(0),
                TransportSendCertainty.NotSubmitted)).ConfigureAwait(false);
          }
        }
        finally
        {
          lock (_gate)
          {
            _connections.Remove(epoch);
          }

          await connection.DisposeAsync().ConfigureAwait(false);
        }
      }
    }
  }

  private ValueTask HandleClosedAsync(SessionEpoch epoch, ConnectionClosed closed)
  {
    if (closed.Epoch != epoch)
    {
      return ValueTask.FromException(
        new InvalidOperationException("A connection close callback carried the wrong epoch."));
    }

    ServerLog.Info(
      "server.connection.closed",
      $"epoch={epoch.Value} reason={ServerLog.Quote(closed.Reason)} " +
      $"lastSequence={closed.LastReceivedSequence.Value} sendCertainty={closed.LastSendCertainty}");
    return _onClosed(closed);
  }

  private async ValueTask HandleConnectionFailureAsync(Exception exception)
  {
    if (exception is Version4PacketFormatException packetFailure) {
      ServerLog.Error("server.connection.failed",
        $"messageId={packetFailure.MessageId} code={packetFailure.ErrorCode} " +
        $"offset={packetFailure.Offset} detail={ServerLog.Quote(packetFailure.Message)}");
    } else {
      ServerLog.Error("server.connection.failed", exception);
    }
    await _onConnectionFailure(exception).ConfigureAwait(false);
  }

  private async ValueTask DispatchPacketAsync(
    InboundVersion4Packet packet,
    CancellationToken cancellationToken)
  {
    lock (_gate)
    {
      if (_packetAdmissionStopped)
      {
        return;
      }

      _acceptedPacketCount++;
    }

    try
    {
      await _onPacket(packet, cancellationToken).ConfigureAwait(false);
    }
    finally
    {
      lock (_gate)
      {
        _acceptedPacketCount--;
        if (_packetAdmissionStopped && _acceptedPacketCount == 0)
        {
          _packetsDrained.TrySetResult(true);
        }
      }
    }
  }
}
