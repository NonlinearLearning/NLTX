namespace Terraria.Network;

public interface IPacketConnection : IAsyncDisposable {
  ConnectionIdentity Identity { get; }
  string ProfileKey { get; }
  ValueTask<PacketMessage?> ReadPacketAsync(CancellationToken cancellationToken = default);
  ValueTask<PacketWriteReceipt> WritePacketAsync<TPacket>(
      TPacket packet, CancellationToken cancellationToken = default);
}
