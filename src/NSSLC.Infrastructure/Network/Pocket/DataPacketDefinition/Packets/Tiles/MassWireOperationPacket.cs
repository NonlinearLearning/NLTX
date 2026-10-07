namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class MassWireOperationPacket
{
    public short StartX { get; set; }
    public short StartY { get; set; }
    public short EndX { get; set; }
    public short EndY { get; set; }
    public byte ToolMode { get; set; }
}
