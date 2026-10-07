namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class PlayerLifeManaPacket
{
    public byte Player { get; set; }
    public short Life { get; set; }
    public short MaximumLife { get; set; }
}
