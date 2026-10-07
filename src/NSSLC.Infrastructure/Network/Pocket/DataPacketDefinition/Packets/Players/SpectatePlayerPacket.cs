namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SpectatePlayerPacket
{
    public byte Player { get; set; }
    public short TargetPlayer { get; set; }
}
