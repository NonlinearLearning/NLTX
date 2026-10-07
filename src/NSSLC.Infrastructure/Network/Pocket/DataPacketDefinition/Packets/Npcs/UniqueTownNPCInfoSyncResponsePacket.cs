namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class UniqueTownNPCInfoSyncResponsePacket
{
    public short NpcIndex { get; set; }
    public string GivenName { get; set; } = "";
    public int Variation { get; set; }
}
