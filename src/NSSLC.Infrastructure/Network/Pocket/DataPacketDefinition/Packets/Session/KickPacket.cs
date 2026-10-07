namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class KickPacket
{
    public NetworkText Text { get; set; } = NetworkText.Literal(string.Empty);
}
