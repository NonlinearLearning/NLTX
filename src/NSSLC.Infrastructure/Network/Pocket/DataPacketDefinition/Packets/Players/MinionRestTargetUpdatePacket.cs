namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class MinionRestTargetUpdatePacket
{
    public byte Player { get; set; }
    public float TargetX { get; set; }
    public float TargetY { get; set; }
}
