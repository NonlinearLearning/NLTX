namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncChestItemPacket
{
    public short ChestIndex { get; set; }
    public byte Slot { get; set; }
    public short Stack { get; set; }
    public byte Prefix { get; set; }
    public short ItemType { get; set; }
}
