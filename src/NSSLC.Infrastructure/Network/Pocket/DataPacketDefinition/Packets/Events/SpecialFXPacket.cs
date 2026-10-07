namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SpecialFXPacket
{
    public byte EffectType { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public byte Parameter { get; set; }
    public short Style { get; set; }
    public byte Flag { get; set; }
}
