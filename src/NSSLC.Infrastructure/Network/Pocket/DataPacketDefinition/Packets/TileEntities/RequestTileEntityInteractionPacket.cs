namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class RequestTileEntityInteractionPacket
{
    public int EntityId { get; set; }
    public byte Player { get; set; }
}
