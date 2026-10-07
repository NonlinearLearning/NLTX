namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncTalkNPCPacket
{
    public byte Player { get; set; }
    public short NpcIndex { get; set; }
}
