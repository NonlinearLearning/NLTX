namespace Terraria.Network;

public interface IPacketHandler<TPacket> {
  ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context, TPacket packet,
      CancellationToken cancellationToken);
}
