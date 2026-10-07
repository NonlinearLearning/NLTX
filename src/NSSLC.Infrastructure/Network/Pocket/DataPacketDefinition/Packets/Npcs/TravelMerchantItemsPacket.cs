namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class TravelMerchantItemsPacket
{
    public IReadOnlyList<short> Items { get; set; } = Array.Empty<short>();
}
