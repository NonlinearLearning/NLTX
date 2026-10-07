namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncProjectilePacket
{
    public short Identity { get; set; }
    public PacketVector2 Position { get; set; }
    public PacketVector2 Velocity { get; set; }
    public byte Owner { get; set; }
    public short ProjectileType { get; set; }
    public float Ai0 { get; set; }
    public float Ai1 { get; set; }
    public ushort? BannerId { get; set; }
    public short? Damage { get; set; }
    public float? Knockback { get; set; }
    public short? OriginalDamage { get; set; }
    public short? ProjectileUuid { get; set; }
    public float Ai2 { get; set; }
}
