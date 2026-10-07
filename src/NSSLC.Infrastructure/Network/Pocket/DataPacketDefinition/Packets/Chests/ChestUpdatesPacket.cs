namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class ChestUpdatesPacket
{
    public byte Action { get; set; }
    public short X { get; set; }
    public short Y { get; set; }
    public short ObjectType { get; set; }
    public short ChestIndex { get; set; }
}
