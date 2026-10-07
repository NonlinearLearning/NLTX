namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncWallPaintOrCoatingPacket
{
    public short X { get; set; }
    public short Y { get; set; }
    public byte PaintOrCoating { get; set; }
    public byte CoatingMode { get; set; }
}
