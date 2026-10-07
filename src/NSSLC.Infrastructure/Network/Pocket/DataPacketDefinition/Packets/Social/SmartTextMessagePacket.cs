namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SmartTextMessagePacket
{
    public PacketRgb Color { get; set; }
    public NetworkText Text { get; set; } = NetworkText.Literal(string.Empty);
    public short WidthLimit { get; set; }
}
