namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed record Packet88ExtraValues(
        byte Flags,
        ushort? Width,
        ushort? Height,
        float? Scale,
        short? Ammo,
        short? UseAmmo,
        bool? NotAmmo);

public sealed partial class ItemTweakerPacket
{
    public short ItemIndex { get; set; }
    public byte Flags { get; set; }
    public uint? Color { get; set; }
    public ushort? Damage { get; set; }
    public float? Knockback { get; set; }
    public ushort? UseAnimation { get; set; }
    public ushort? UseTime { get; set; }
    public short? Shoot { get; set; }
    public float? ShootSpeed { get; set; }
    public Packet88ExtraValues? Extra { get; set; }
}
