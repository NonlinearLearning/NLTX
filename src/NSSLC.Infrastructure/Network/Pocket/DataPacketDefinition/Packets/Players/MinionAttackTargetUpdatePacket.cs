namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class MinionAttackTargetUpdatePacket
{
    public byte Player { get; set; }
    public short NpcIndex { get; set; }
}
