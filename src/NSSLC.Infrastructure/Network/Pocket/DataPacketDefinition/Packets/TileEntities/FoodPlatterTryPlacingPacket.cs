namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class FoodPlatterTryPlacingPacket
{
    public short X { get; set; }
    public short Y { get; set; }
    public short ItemType { get; set; }
    public byte Prefix { get; set; }
    public short Stack { get; set; }
}
