namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncChestSizePacket
{
    public short ChestIndex { get; set; }
    public short Size { get; set; }
}
