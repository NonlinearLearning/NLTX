using Terraria.Network;

namespace NSSLC.NetworkVerification;

internal sealed class RecordingHandler<TPacket> : IPacketHandler<TPacket> {
  private readonly Func<NetworkSessionContext, TPacket, CancellationToken,
      ValueTask<PacketHandlingResult>> _handle;

  public RecordingHandler(Func<NetworkSessionContext, TPacket, CancellationToken,
      ValueTask<PacketHandlingResult>> handle) {
    _handle = handle;
  }

  public ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context, TPacket packet,
      CancellationToken cancellationToken) {
    return _handle.Invoke(context, packet, cancellationToken);
  }
}
