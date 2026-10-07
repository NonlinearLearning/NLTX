namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncItemsWithShimmerPacket
{
    public PacketItemSyncData Item { get; set; } = new(0, 0, 0, 0, 0, 0, 0, 0, 0);
    public bool Shimmered { get; set; }
    public float ShimmerTime { get; set; }
}
