namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class ChestNameResponsePacket
{
    public short ChestIndex { get; set; }
    public short X { get; set; }
    public short Y { get; set; }
    public string ChestName { get; set; } = "";
}
