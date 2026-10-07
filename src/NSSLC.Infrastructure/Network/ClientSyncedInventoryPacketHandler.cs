using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.Infrastructure.Network;

/// <summary>Consumes the client's empty inventory-sync completion notice without granting state.</summary>
public sealed class ClientSyncedInventoryPacketHandler : IPacketHandler<ClientSyncedInventoryPacket> {
  private const NetworkSessionStage AllowedStages = NetworkSessionStage.AwaitPlayerData
      | NetworkSessionStage.Active;

  public ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      ClientSyncedInventoryPacket packet, CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    if ((AllowedStages & context.Stage) == 0) {
      return Rejected("InventorySyncMarkerRequiresBoundSession");
    }
    if (!packet.Bytes.IsEmpty) {
      return Rejected("InventorySyncMarkerMustBeEmpty");
    }

    return ValueTask.FromResult(new PacketHandlingResult(true));
  }

  private static ValueTask<PacketHandlingResult> Rejected(string code) {
    return ValueTask.FromResult(new PacketHandlingResult(false, rejectionCode: code));
  }
}
