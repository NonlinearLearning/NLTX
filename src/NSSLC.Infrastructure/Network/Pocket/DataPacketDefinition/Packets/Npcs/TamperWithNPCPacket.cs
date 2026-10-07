namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class TamperWithNPCPacket
{
    public ushort NpcIndex { get; set; }
    public byte Action { get; set; }
    public int? Value { get; set; }
    public short? Style { get; set; }
}
