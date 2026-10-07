namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class CombatTextIntPacket
{
    public float X { get; set; }
    public float Y { get; set; }
    public PacketRgb Color { get; set; }
    public int Amount { get; set; }
}
