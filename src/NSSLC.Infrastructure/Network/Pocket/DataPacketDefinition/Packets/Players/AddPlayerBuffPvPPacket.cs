namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class AddPlayerBuffPvPPacket
{
    public byte Player { get; set; }
    public ushort BuffType { get; set; }
    public int BuffTime { get; set; }
}
