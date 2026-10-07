namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class TeleportNPCThroughPortalPacket
{
    public ushort NpcIndex { get; set; }
    public short PortalColorIndex { get; set; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float VelocityX { get; set; }
    public float VelocityY { get; set; }
}
