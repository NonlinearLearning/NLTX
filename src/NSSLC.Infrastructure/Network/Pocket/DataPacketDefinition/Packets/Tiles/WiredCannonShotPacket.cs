namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class WiredCannonShotPacket
{
    public short Damage { get; set; }
    public float Knockback { get; set; }
    public short X { get; set; }
    public short Y { get; set; }
    public short Angle { get; set; }
    public short Ammo { get; set; }
    public byte Owner { get; set; }
}
