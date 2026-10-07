namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class TEDisplayDollDataSyncPacket
{
    public byte Player { get; set; }
    public int EntityId { get; set; }
    public byte ItemIndex { get; set; }
    public byte Command { get; set; }
    public PacketTileEntityItem? Item { get; set; }
    public byte? Pose { get; set; }
}
