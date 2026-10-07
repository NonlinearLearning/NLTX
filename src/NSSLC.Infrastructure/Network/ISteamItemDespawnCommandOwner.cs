using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.Infrastructure.Network;

/// <summary>Applies client-originated Steam packet 151 to authoritative world-item state.</summary>
public interface ISteamItemDespawnCommandOwner
{
  ValueTask<PacketHandlingResult> ApplyDespawnItemAsync(
    NetworkSessionContext context,
    SyncItemDespawnPacket packet,
    CancellationToken cancellationToken);
}
