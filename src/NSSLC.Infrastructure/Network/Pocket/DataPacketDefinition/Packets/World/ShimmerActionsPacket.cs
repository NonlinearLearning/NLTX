namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class ShimmerActionsPacket
{
    public byte Action { get; set; }
    public PacketVector2? Position { get; set; }
    public int? CoinAmount { get; set; }
}
