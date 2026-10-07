namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncPlayerZonePacket
{
    public byte Player { get; set; }
    public byte Zone1 { get; set; }
    public byte Zone2 { get; set; }
    public byte Zone3 { get; set; }
    public byte Zone4 { get; set; }
    public byte Zone5 { get; set; }
    public byte TownNpcCount { get; set; }
}
