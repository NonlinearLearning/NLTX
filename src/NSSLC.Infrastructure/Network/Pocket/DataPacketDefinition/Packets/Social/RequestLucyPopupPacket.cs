namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class RequestLucyPopupPacket
{
    public byte MessageSource { get; set; }
    public byte Variation { get; set; }
    public PacketVector2 Velocity { get; set; }
    public int PositionX { get; set; }
    public int PositionY { get; set; }
}
