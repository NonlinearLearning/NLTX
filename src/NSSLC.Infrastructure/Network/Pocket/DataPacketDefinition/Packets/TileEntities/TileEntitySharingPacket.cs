namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class TileEntitySharingPacket
{
    public int EntityId { get; set; }
    public bool Exists { get; set; }
    public PacketTileEntityRecord? Entity { get; set; }
}
