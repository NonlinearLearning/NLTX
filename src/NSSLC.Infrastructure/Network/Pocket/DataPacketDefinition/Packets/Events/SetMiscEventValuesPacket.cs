namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SetMiscEventValuesPacket
{
    public byte EventKind { get; set; }
    public int Value { get; set; }
}
