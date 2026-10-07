namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class ChestNameRequestPacket
{
    public short ChestIndex { get; set; }
    public short X { get; set; }
    public short Y { get; set; }
}
