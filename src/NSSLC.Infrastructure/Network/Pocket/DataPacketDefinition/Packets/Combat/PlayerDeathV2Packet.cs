namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class PlayerDeathV2Packet
{
    public byte Player { get; set; }
    public PlayerDeathReason Reason { get; set; } = new();
    public short Damage { get; set; }
    public byte DirectionCode { get; set; } = 1;
    public byte DeathFlags { get; set; }
}
