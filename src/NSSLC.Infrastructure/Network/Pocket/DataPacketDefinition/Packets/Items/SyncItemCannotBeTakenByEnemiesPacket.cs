namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncItemCannotBeTakenByEnemiesPacket
{
    public PacketItemSyncData Item { get; set; } = new(0, 0, 0, 0, 0, 0, 0, 0, 0);
    public byte CannotBeTakenTimer { get; set; }
}
