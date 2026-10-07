namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SetTimePacket
{
    public byte DayTime { get; set; }
    public int Time { get; set; }
    public short SunModY { get; set; }
    public short MoonModY { get; set; }
}
