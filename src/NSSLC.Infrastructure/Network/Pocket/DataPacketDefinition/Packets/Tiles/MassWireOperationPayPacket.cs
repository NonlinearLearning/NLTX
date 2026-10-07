namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class MassWireOperationPayPacket
{
    public short ItemType { get; set; }
    public short Count { get; set; }
    public byte Player { get; set; }
}
