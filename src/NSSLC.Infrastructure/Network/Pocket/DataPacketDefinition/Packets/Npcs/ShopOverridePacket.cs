namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class ShopOverridePacket
{
    public byte Player { get; set; }
    public short NpcType { get; set; }
    public float PriceAdjustment { get; set; }
    public byte ShopId { get; set; }
    public int SpecialCurrency { get; set; }
    public byte ShopType { get; set; }
}
