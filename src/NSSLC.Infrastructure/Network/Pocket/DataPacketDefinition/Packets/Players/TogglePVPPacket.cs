namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class TogglePVPPacket
{
    public byte Player { get; set; }
    public bool Hostile { get; set; }
}
