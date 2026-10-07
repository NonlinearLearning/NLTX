namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class InstancedItemPacket
{
    public short ItemIndex { get; set; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float VelocityX { get; set; }
    public float VelocityY { get; set; }
    public short Stack { get; set; }
    public byte Prefix { get; set; }
    public byte StateFlags { get; set; }
    public short ItemType { get; set; }
    public bool? Shimmered { get; set; }
    public float? ShimmerTime { get; set; }
    public byte? EnemyGrabDelayTime { get; set; }
}
