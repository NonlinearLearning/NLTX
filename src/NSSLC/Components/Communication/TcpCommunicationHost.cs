using System.Net;
using System.Net.Sockets;

namespace Terraria.Communication;

/// <summary>Owns the TCP listener and delegates each framed connection to communication callbacks.</summary>
public sealed class TcpCommunicationHost : ICommunicationTransportPort, IAsyncDisposable
{
  private readonly object _gate = new();
  private readonly TcpListenerAdapter _listener;
  private readonly Func<SessionEpoch> _allocateEpoch;
  private readonly Func<InboundRawFrame, InboundPacketEnvelope> _createEnvelope;
  private readonly Func<InboundCommunicationPacket, CancellationToken, ValueTask> _onPacket;
  private readonly Func<ConnectionClosed, ValueTask> _onClosed;
  private readonly Func<Exception, ValueTask> _onConnectionFailure;
  private readonly Func<SessionEpoch, ValueTask>? _onConnected;
  private readonly PacketTcpConnectionOptions? _connectionOptions;
  private readonly TimeProvider? _timeProvider;
  private readonly Dictionary<SessionEpoch, PacketTcpConnection> _connections = [];
  private Task? _runTask;
  private Task? _disposeTask;
  private bool _stopRequested;
  private bool _disposed;

  public TcpCommunicationHost(
      IPEndPoint endpoint,
      Func<SessionEpoch> allocateEpoch,
      Func<InboundRawFrame, InboundPacketEnvelope> createEnvelope,
      Func<InboundCommunicationPacket, CancellationToken, ValueTask> onPacket,
      Func<ConnectionClosed, ValueTask> onClosed,
      Func<Exception, ValueTask> onConnectionFailure,
      TcpListenerAdapterOptions? listenerOptions = null,
      PacketTcpConnectionOptions? connectionOptions = null,
      TimeProvider? timeProvider = null,
      Func<SessionEpoch, ValueTask>? onConnected = null)
  {
    ArgumentNullException.ThrowIfNull(endpoint);
    ArgumentNullException.ThrowIfNull(allocateEpoch);
    ArgumentNullException.ThrowIfNull(createEnvelope);
    ArgumentNullException.ThrowIfNull(onPacket);
    ArgumentNullException.ThrowIfNull(onClosed);
    ArgumentNullException.ThrowIfNull(onConnectionFailure);
    _listener = new TcpListenerAdapter(endpoint, listenerOptions);
    _allocateEpoch = allocateEpoch;
    _createEnvelope = createEnvelope;
    _onPacket = onPacket;
    _onClosed = onClosed;
    _onConnectionFailure = onConnectionFailure;
    _onConnected = onConnected;
    _connectionOptions = connectionOptions;
    _timeProvider = timeProvider;
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

  public Task StartAsync(CancellationToken cancellationToken = default)
  {
    lock (_gate)
    {
      ObjectDisposedException.ThrowIf(_disposed, this);
      if (_stopRequested || _runTask is not null)
      {
        throw new InvalidOperationException("A stopped communication host cannot be restarted.");
      }

      _runTask = _listener.RunAsync(
          HandleConnectionAsync,
          HandleConnectionFailureAsync,
          cancellationToken);
    }

    return Task.CompletedTask;
  }

  public async ValueTask<IReadOnlyList<SendReceipt>> SendAsync(
      OutboundPacketIntent intent,
      CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(intent);
    PacketTcpConnection[] targets;
    lock (_gate)
    {
      targets = intent.TargetEpochs
          .Where(epoch => intent.IgnoreEpoch != epoch)
          .Where(epoch => _connections.ContainsKey(epoch))
          .Select(epoch => _connections[epoch])
          .ToArray();
    }

    var receipts = new List<SendReceipt>(targets.Length);
    foreach (PacketTcpConnection target in targets)
    {
      receipts.Add(await target.SendAsync(intent, cancellationToken).ConfigureAwait(false));
    }

    return receipts.AsReadOnly();
  }

  public ValueTask CloseAsync(SessionEpoch epoch)
  {
    lock (_gate)
    {
      if (_connections.TryGetValue(epoch, out PacketTcpConnection? connection))
      {
        connection.RequestClose("LocalRequest");
      }
    }

    return ValueTask.CompletedTask;
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
    if (runTask is not null)
    {
      await runTask.ConfigureAwait(false);
    }
  }

  public ValueTask DisposeAsync()
  {
    lock (_gate)
    {
      if (_disposeTask is null)
      {
        _disposed = true;
        _disposeTask = DisposeCoreAsync();
      }

      return new ValueTask(_disposeTask);
    }
  }

  private async Task DisposeCoreAsync()
  {
    await StopAsync().ConfigureAwait(false);
    await _listener.DisposeAsync().ConfigureAwait(false);
  }

  private async Task HandleConnectionAsync(Socket socket, CancellationToken cancellationToken)
  {
    SessionEpoch epoch = _allocateEpoch();
    if (!epoch.IsValid)
    {
      throw new InvalidOperationException("The communication epoch allocator returned an invalid epoch.");
    }

    PacketTcpConnection? connection = null;
    bool registered = false;
    try
    {
      connection = new PacketTcpConnection(
          socket,
          epoch,
          HandleFrameAsync,
          _connectionOptions,
          _timeProvider,
          closed => HandleClosedAsync(epoch, closed));
      lock (_gate)
      {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_stopRequested || !_connections.TryAdd(epoch, connection))
        {
          throw new InvalidOperationException("The communication host is stopping or epoch is duplicated.");
        }

        registered = true;
      }

      if (_onConnected is not null)
      {
        await _onConnected(epoch).ConfigureAwait(false);
      }

      await connection.RunAsync(cancellationToken).ConfigureAwait(false);
    }
    finally
    {
      if (connection is not null)
      {
        if (registered)
        {
          lock (_gate)
          {
            _connections.Remove(epoch);
          }
        }

        await connection.DisposeAsync().ConfigureAwait(false);
      }
    }
  }

  private ValueTask HandleFrameAsync(InboundRawFrame frame, CancellationToken cancellationToken)
  {
    InboundPacketEnvelope envelope = _createEnvelope(frame);
    return _onPacket(new InboundCommunicationPacket(envelope, frame.Frame), cancellationToken);
  }

  private ValueTask HandleClosedAsync(SessionEpoch epoch, ConnectionClosed closed)
  {
    if (closed.Epoch != epoch)
    {
      return ValueTask.FromException(
          new InvalidOperationException("A close callback carried the wrong epoch."));
    }

    return _onClosed(closed);
  }

  private ValueTask HandleConnectionFailureAsync(Exception exception)
  {
    return _onConnectionFailure(exception);
  }
}
