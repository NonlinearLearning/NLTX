namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncLoadoutPacket
{
    public byte Player { get; set; }
    public byte LoadoutIndex { get; set; }
    public ushort AccessoryVisibilityMask { get; set; }
}
