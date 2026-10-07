namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class StatusTextSizePacket
{
    public int Value { get; set; }
    public NetworkText Text { get; set; } = NetworkText.Literal(string.Empty);
    public byte Flags { get; set; }
}
