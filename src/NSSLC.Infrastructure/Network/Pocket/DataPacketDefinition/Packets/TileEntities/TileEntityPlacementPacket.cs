namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class TileEntityPlacementPacket
{
    public short X { get; set; }
    public short Y { get; set; }
    public byte EntityType { get; set; }
}
