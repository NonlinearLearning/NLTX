namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class ToggleDoorStatePacket
{
    public byte Action { get; set; }
    public short X { get; set; }
    public short Y { get; set; }
    public byte Direction { get; set; }
}
