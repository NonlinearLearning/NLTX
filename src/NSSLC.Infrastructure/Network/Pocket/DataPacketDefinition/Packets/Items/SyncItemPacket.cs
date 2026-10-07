namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncItemPacket
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

    // Steam 1.4.5.8 conditionally appends these values when StateFlags bit 2/3 is set.
    // The generated TerrariaV4 codec keeps its legacy 24-byte base layout; the Steam
    // profile uses SteamItemPacketCodec to read and write the conditional extension.
    public bool? Shimmered { get; set; }
    public float? ShimmerTime { get; set; }
    public byte? EnemyGrabDelayTime { get; set; }
}
