namespace Terraria.Items.InventoryContainers;

public sealed class ItemProgressionInteractionCapabilityComponent
{
  public ItemProgressionInteractionCapabilityComponent(
    bool questItem,
    bool flame,
    bool mech,
    int tileWand,
    int fishingPole,
    int bait,
    short makeNpc,
    bool expertOnly,
    bool expert)
  {
    QuestItem = questItem;
    Flame = flame;
    Mech = mech;
    TileWand = tileWand;
    FishingPole = fishingPole;
    Bait = bait;
    MakeNpc = makeNpc;
    ExpertOnly = expertOnly;
    Expert = expert;
  }

  public bool QuestItem { get; }
  public bool Flame { get; }
  public bool Mech { get; }
  public int TileWand { get; }
  public int FishingPole { get; }
  public int Bait { get; }
  public short MakeNpc { get; }
  public bool ExpertOnly { get; }
  public bool Expert { get; }
}
