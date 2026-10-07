namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed record PacketTrackedProjectileRef(short Owner, short? Identity, short? Type);

public sealed partial class SyncProjectileTrackersPacket
{
    public byte Player { get; set; }
    public PacketTrackedProjectileRef PiggyBank { get; set; } = new(-1, null, null);
    public PacketTrackedProjectileRef VoidLens { get; set; } = new(-1, null, null);
}
