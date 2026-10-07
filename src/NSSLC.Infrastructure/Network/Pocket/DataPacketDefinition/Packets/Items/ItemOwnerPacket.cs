namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class ItemOwnerPacket
{
    public short ItemIndex { get; set; }
    public byte ReservedForPlayer { get; set; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }

    // Steam 1.4.5.8 appends these ownership timers and identity around the position.
    // The TerrariaV4 packet graph retains its old 11-byte layout.
    public int TimeToKeepReservation { get; set; }
    public byte GrabDelayPlayer { get; set; }
    public int GrabDelayTime { get; set; }
}
