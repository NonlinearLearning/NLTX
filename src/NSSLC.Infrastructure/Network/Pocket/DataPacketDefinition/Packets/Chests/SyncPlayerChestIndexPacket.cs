namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncPlayerChestIndexPacket
{
    public byte Player { get; set; }
    public short ChestIndex { get; set; }
}
