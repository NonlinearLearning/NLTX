namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class CombatTextStringPacket
{
    public float X { get; set; }
    public float Y { get; set; }
    public PacketRgb Color { get; set; }
    public NetworkText Text { get; set; } = NetworkText.Literal(string.Empty);
}
