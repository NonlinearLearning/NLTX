namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class DamageNPCPacket
{
    public short NpcIndex { get; set; }
    public short Damage { get; set; }
    public float Knockback { get; set; }
    public byte EncodedDirection { get; set; }
    public byte HitDirection { get; set; }
}
