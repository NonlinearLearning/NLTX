namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncPlayerPacket
{
    public byte Player { get; set; }
    public byte SkinVariant { get; set; }
    public byte VoiceVariant { get; set; }
    public float VoicePitchOffset { get; set; }
    public byte Hair { get; set; }
    public string Name { get; set; } = string.Empty;
    public byte HairDye { get; set; }
    public ushort HiddenAccessories { get; set; }
    public byte HideMisc { get; set; }
    public PacketRgb HairColor { get; set; }
    public PacketRgb SkinColor { get; set; }
    public PacketRgb EyeColor { get; set; }
    public PacketRgb ShirtColor { get; set; }
    public PacketRgb UnderShirtColor { get; set; }
    public PacketRgb PantsColor { get; set; }
    public PacketRgb ShoeColor { get; set; }
    public byte DifficultyAndAccessoryFlags { get; set; }
    public byte BiomeAndCartFlags { get; set; }
    public byte PermanentUpgradeFlags { get; set; }
}
