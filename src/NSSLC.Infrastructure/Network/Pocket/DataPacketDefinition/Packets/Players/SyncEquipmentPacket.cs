namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncEquipmentPacket
{
    public byte Player { get; set; }
    public short Slot { get; set; }
    public short Stack { get; set; }
    public byte Prefix { get; set; }
    public short ItemType { get; set; }
    public bool Favorited { get; set; }
    public bool IsCreativeItem { get; set; }
}
