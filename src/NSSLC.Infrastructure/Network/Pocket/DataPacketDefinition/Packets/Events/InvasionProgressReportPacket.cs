namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class InvasionProgressReportPacket
{
    public int InvasionType { get; set; }
    public int Progress { get; set; }
    public sbyte Wave { get; set; }
    public sbyte MaxWave { get; set; }
}
