namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class RequestNPCBuffRemovalPacket
{
    public short NpcIndex { get; set; }
    public ushort BuffType { get; set; }
}
