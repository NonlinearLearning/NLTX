namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class TeamChangePacket
{
    public byte Player { get; set; }
    public byte Team { get; set; }
}
