namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class AddNPCBuffPacket
{
    public short NpcIndex { get; set; }
    public ushort BuffType { get; set; }
    public short BuffTime { get; set; }
}
