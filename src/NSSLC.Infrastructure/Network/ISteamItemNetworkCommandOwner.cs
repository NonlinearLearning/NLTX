using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.Infrastructure.Network;

/// <summary>
/// Applies a client-originated Steam 21 item update against authoritative world-item
/// state. Implementations own slot-to-entity resolution, provenance, and reservation
/// checks; the packet handler does not mutate item state itself.
/// </summary>
public interface ISteamItemNetworkCommandOwner
{
  ValueTask<PacketHandlingResult> ApplySyncItemAsync(
    NetworkSessionContext context,
    SyncItemPacket packet,
    CancellationToken cancellationToken);
}
