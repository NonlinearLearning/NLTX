namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncExtraValuePacket
{
    public short NpcIndex { get; set; }
    public int ExtraValue { get; set; }
    public float ValueX { get; set; }
    public float ValueY { get; set; }
}
