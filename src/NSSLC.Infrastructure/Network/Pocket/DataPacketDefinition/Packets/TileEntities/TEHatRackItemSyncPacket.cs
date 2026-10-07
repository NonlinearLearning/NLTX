namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class TEHatRackItemSyncPacket
{
    public byte Player { get; set; }
    public int EntityId { get; set; }
    public byte EncodedSlot { get; set; }
    public PacketTileEntityItem Item { get; set; }
}
