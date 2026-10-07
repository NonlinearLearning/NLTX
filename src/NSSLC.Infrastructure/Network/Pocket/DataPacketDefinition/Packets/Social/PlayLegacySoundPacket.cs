namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class PlayLegacySoundPacket
{
    public PacketVector2 Position { get; set; }
    public ushort SoundIndex { get; set; }
    public int? Style { get; set; }
    public float? Volume { get; set; }
    public float? PitchOffset { get; set; }
}
