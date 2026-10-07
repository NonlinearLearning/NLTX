namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class TemporaryAnimationPacket
{
    public short AnimationType { get; set; }
    public ushort TileType { get; set; }
    public short X { get; set; }
    public short Y { get; set; }
}
