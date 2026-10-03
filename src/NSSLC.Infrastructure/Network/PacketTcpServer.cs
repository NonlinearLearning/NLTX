using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using NetCoreServer;

namespace NSSLC.Infrastructure.Network;

public sealed class PacketTcpServer : IAsyncDisposable {
  private sealed class ServerAdapter : TcpServer {
    private readonly PacketTcpServer _owner;

    public ServerAdapter(PacketTcpServer owner, IPAddress address, int port) : base(address, port) {
      _owner = owner;
    }

    protected override TcpSession CreateSession() {
      return new NetCoreServerSessionAdapter(this, _owner._profile, _owner._options,
          _owner._budget, Interlocked.Increment(ref _owner._epoch), _owner.Ready);
    }

    protected override void OnError(SocketError error) {
      _owner._lastError = new SocketException((int)error);
    }
  }

  private readonly ProtocolProfile _profile;
  private readonly PacketConnectionOptions _options;
  private readonly PacketByteBudget _budget;
  private readonly Func<PacketConnection, CancellationToken, Task> _handle;
  private readonly ConcurrentDictionary<PacketConnection, Task> _sessions = new();
  private readonly object _sessionGate = new();
  private readonly CancellationTokenSource _stopping = new();
  private readonly ServerAdapter _server;
  private Exception? _lastError;
  private long _epoch;
  private int _disposed;

  public EndPoint EndPoint => _server.Endpoint;
  public Exception? LastError => _lastError;

  public PacketTcpServer(IPAddress address, int port, ProtocolProfile profile,
      Func<PacketConnection, CancellationToken, Task> handle,
      PacketConnectionOptions? options = null, PacketByteBudget? budget = null) {
    ArgumentNullException.ThrowIfNull(handle);
    _profile = profile;
    _options = options ?? new();
    _options.Validate();
    _budget = budget ?? new();
    _handle = handle;
    _server = new(this, address, port);
  }

  public void Start() {
    ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
    if (!_server.Start()) {
      throw new IOException("TCP listener could not start.", _lastError);
    }
  }

  public async ValueTask DisposeAsync() {
    if (Interlocked.Exchange(ref _disposed, 1) != 0) {
      return;
    }
    _stopping.Cancel();
    _server.Stop();
    foreach (PacketConnection connection in _sessions.Keys) {
      await connection.DisposeAsync().ConfigureAwait(false);
    }
    await Task.WhenAll(_sessions.Values).ConfigureAwait(false);
    _server.Dispose();
    _stopping.Dispose();
  }

  private void Ready(PacketConnection connection) {
    var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    lock (_sessionGate) {
      if (Volatile.Read(ref _disposed) != 0) {
        connection.NotifyClosed(connection.Identity.Epoch);
        return;
      }
      _sessions.TryAdd(connection, completion.Task);
    }
    Task task;
    try {
      // Admission is installed before the library begins receiving bytes.
      task = _handle.Invoke(connection, _stopping.Token);
    } catch (Exception error) {
      task = Task.FromException(error);
    }
    _ = FinishSessionAsync(connection, task, completion);
  }

  private async Task FinishSessionAsync(PacketConnection connection, Task task,
      TaskCompletionSource completion) {
    try {
      await task.ConfigureAwait(false);
    } catch (OperationCanceledException) when (_stopping.IsCancellationRequested) {
    } catch (Exception error) {
      _lastError = error;
    } finally {
      try {
        await connection.DisposeAsync().ConfigureAwait(false);
      } finally {
        completion.TrySetResult();
        _sessions.TryRemove(connection, out _);
      }
    }
  }
}
