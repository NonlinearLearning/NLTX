namespace Terraria.Items.InventoryContainers;

public sealed class ItemBuffMountConsumableCapabilityComponent
{
  public ItemBuffMountConsumableCapabilityComponent(
    int buffType,
    int buffTime,
    int mountType,
    bool cartTrack,
    bool chlorophyteExtractinatorConsumable,
    bool dd2Summon)
  {
    BuffType = buffType;
    BuffTime = buffTime;
    MountType = mountType;
    CartTrack = cartTrack;
    ChlorophyteExtractinatorConsumable = chlorophyteExtractinatorConsumable;
    DD2Summon = dd2Summon;
  }

  public int BuffType { get; }
  public int BuffTime { get; }
  public int MountType { get; }
  public bool CartTrack { get; }
  public bool ChlorophyteExtractinatorConsumable { get; }
  public bool DD2Summon { get; }
}
