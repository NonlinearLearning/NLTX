namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class PlayerHealPacket
{
    public byte Player { get; set; }
    public short HealAmount { get; set; }
}
