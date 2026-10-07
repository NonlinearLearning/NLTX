namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class TeleportPlayerThroughPortalPacket
{
    public byte Player { get; set; }
    public short PortalColorIndex { get; set; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float VelocityX { get; set; }
    public float VelocityY { get; set; }
}
