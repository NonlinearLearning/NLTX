namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class TeleportEntityPacket
{
    public byte EntityKind { get; set; }
    public short EntityIndex { get; set; }
    public PacketVector2 Position { get; set; }
    public byte Style { get; set; }
    public bool UseCurrentEntityPosition { get; set; }
    public int? TeleportValue { get; set; }
}
