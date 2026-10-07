namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class TileManipulationPacket
{
    public byte Action { get; set; }
    public short X { get; set; }
    public short Y { get; set; }
    public short TileOrWallType { get; set; }
    public byte Style { get; set; }
}
