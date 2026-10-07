namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class TeamChangeFromUIPacket
{
    public byte Player { get; set; }
    public byte Team { get; set; }
}
