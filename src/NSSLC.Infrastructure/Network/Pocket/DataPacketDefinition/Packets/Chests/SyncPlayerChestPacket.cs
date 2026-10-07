namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncPlayerChestPacket
{
    public short ChestIndex { get; set; }
    public short ChestX { get; set; }
    public short ChestY { get; set; }
    public byte NameLengthIndicator { get; set; }
    public string? Name { get; set; }
}
