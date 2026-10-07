namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncRevengeMarkerPacket
{
    public int UniqueId { get; set; }
    public PacketVector2 Position { get; set; }
    public int NpcNetId { get; set; }
    public float NpcHpPercent { get; set; }
    public int NpcType { get; set; }
    public int NpcAiStyle { get; set; }
    public int CoinsValue { get; set; }
    public float BaseValue { get; set; }
    public bool SpawnedFromStatue { get; set; }
}
