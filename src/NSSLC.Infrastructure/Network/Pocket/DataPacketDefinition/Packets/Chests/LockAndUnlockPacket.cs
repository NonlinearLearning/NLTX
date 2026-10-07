namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class LockAndUnlockPacket
{
    public byte Action { get; set; }
    public short X { get; set; }
    public short Y { get; set; }
}
