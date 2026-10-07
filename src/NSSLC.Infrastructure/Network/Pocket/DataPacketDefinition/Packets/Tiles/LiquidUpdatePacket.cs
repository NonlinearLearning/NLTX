namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class LiquidUpdatePacket
{
    public short X { get; set; }
    public short Y { get; set; }
    public byte LiquidAmount { get; set; }
    public byte LiquidType { get; set; }
}
