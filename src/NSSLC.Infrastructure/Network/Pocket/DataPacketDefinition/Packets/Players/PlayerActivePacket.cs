namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class PlayerActivePacket
{
    public byte Player { get; set; }
    public byte ActiveState { get; set; }
}
