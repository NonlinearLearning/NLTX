namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class ItemOwnerPacket
{
    public short ItemIndex { get; set; }
    public byte ReservedForPlayer { get; set; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }

    // Steam 1.4.5.8 appends these ownership timers and identity around the position.
    public int TimeToKeepReservation { get; set; }
    public byte GrabDelayPlayer { get; set; } = byte.MaxValue;
    public int GrabDelayTime { get; set; }
}
