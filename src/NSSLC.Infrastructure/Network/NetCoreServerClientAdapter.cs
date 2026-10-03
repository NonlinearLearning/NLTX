using System.Net.Sockets;
using Terraria.Network;

namespace NSSLC.Infrastructure.Network;

internal sealed class NetCoreServerClientAdapter : NetCoreServer.TcpClient, IPacketTransport {
  private readonly TaskCompletionSource _connected = new(
      TaskCreationOptions.RunContinuationsAsynchronously);
  private int _closing;
  private Socket? _ownedSocket;

  public PacketConnection Connection { get; }
  public Task Connected => _connected.Task;

  public NetCoreServerClientAdapter(string address, int port, ProtocolProfile profile,
      PacketConnectionOptions options, PacketByteBudget budget, ConnectionIdentity identity)
      : base(address, port) {
    OptionSendBufferLimit = options.MaximumFrameBytes;
    OptionReceiveBufferLimit = options.ReceiveBytes;
    Connection = new(identity, this, profile, PacketDirection.ServerToClient, options, budget);
  }

  protected override Socket CreateSocket() {
    _ownedSocket = base.CreateSocket();
    return _ownedSocket;
  }

  protected override void Dispose(bool disposingManagedResources) {
    Interlocked.Exchange(ref _closing, 1);
    base.Dispose(disposingManagedResources);
    if (disposingManagedResources) {
      // The library does not dispose a socket after an already-failed connect.
      _ownedSocket?.Dispose();
    }
  }

  bool IPacketTransport.Submit(ReadOnlySpan<byte> frame) {
    return SendAsync(frame);
  }

  void IPacketTransport.Close() {
    Interlocked.Exchange(ref _closing, 1);
    DisconnectAsync();
  }

  protected override void OnConnected() {
    if (Volatile.Read(ref _closing) != 0) {
      DisconnectAsync();
      return;
    }
    _connected.TrySetResult();
  }

  protected override void OnDisconnected() {
    _connected.TrySetException(new IOException("Connection closed before establishment."));
    Connection.NotifyClosed(Connection.Identity.Epoch);
  }

  protected override void OnReceived(byte[] buffer, long offset, long size) {
    Connection.ReceiveBytes(Connection.Identity.Epoch,
        buffer.AsSpan(checked((int)offset), checked((int)size)));
  }

  protected override void OnSent(long sent, long pending) {
    Connection.NotifySent(Connection.Identity.Epoch, sent);
  }

  protected override void OnError(SocketError error) {
    var failure = new SocketException((int)error);
    _connected.TrySetException(failure);
    Connection.NotifyClosed(Connection.Identity.Epoch, failure);
  }
}
