namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class PlaceObjectPacket
{
    public short X { get; set; }
    public short Y { get; set; }
    public short ObjectType { get; set; }
    public short Style { get; set; }
    public byte Alternate { get; set; }
    public sbyte Random { get; set; }
    public bool DirectionPositive { get; set; }
}
