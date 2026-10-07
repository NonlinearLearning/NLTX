namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class NebulaLevelupRequestPacket
{
    public byte Player { get; set; }
    public ushort ItemType { get; set; }
    public PacketVector2 Position { get; set; }
}
