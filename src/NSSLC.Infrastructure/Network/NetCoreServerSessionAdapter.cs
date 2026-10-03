using System.Net.Sockets;
using NetCoreServer;
using Terraria.Network;

namespace NSSLC.Infrastructure.Network;

internal sealed class NetCoreServerSessionAdapter : TcpSession, IPacketTransport {
  private readonly Action<PacketConnection> _ready;
  public PacketConnection Connection { get; }

  public NetCoreServerSessionAdapter(TcpServer server, ProtocolProfile profile,
      PacketConnectionOptions options, PacketByteBudget budget, long epoch,
      Action<PacketConnection> ready) : base(server) {
    _ready = ready;
    OptionSendBufferLimit = options.MaximumFrameBytes;
    OptionReceiveBufferLimit = options.ReceiveBytes;
    Connection = new(new(Guid.NewGuid(), epoch), this, profile,
        PacketDirection.ClientToServer, options, budget);
  }

  bool IPacketTransport.Submit(ReadOnlySpan<byte> frame) {
    return SendAsync(frame);
  }

  void IPacketTransport.Close() {
    Disconnect();
  }

  protected override void OnConnecting() {
    _ready.Invoke(Connection);
  }

  protected override void OnReceived(byte[] buffer, long offset, long size) {
    Connection.ReceiveBytes(Connection.Identity.Epoch,
        buffer.AsSpan(checked((int)offset), checked((int)size)));
  }

  protected override void OnSent(long sent, long pending) {
    Connection.NotifySent(Connection.Identity.Epoch, sent);
  }

  protected override void OnDisconnected() {
    Connection.NotifyClosed(Connection.Identity.Epoch);
  }

  protected override void OnError(SocketError error) {
    Connection.NotifyClosed(Connection.Identity.Epoch, new SocketException((int)error));
  }
}
