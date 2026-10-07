namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class LandGolfBallInCupPacket
{
    public byte Player { get; set; }
    public ushort BallX { get; set; }
    public ushort BallY { get; set; }
    public ushort CupX { get; set; }
    public ushort CupY { get; set; }
}
