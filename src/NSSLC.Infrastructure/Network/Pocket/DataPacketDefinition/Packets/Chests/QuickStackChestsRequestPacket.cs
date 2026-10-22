namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class QuickStackChestsRequestPacket
{
    public short[] ItemSlots { get; set; } = [];
    public bool SmartStack { get; set; }
}
