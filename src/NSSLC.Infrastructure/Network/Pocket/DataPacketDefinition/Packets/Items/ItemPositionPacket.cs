namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class ItemPositionPacket
{
    public short ItemIndex { get; set; }
    public PacketVector2 Position { get; set; }
}
