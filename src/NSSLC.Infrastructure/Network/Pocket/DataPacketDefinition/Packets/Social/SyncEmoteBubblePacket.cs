namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncEmoteBubblePacket
{
    public int BubbleId { get; set; }
    public byte AnchorKind { get; set; } = 255;
    public ushort? AnchorId { get; set; }
    public ushort? Lifetime { get; set; }
    public byte? Emote { get; set; }
    public short? Metadata { get; set; }
}
