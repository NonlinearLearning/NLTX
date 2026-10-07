namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncTilePickingPacket
{
    public byte Player { get; set; }
    public short X { get; set; }
    public short Y { get; set; }
    public byte TileType { get; set; }
}
