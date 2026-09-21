namespace Terraria.Items.InventoryContainers;

public sealed class ItemEquipmentAppearanceComponent
{
  public ItemEquipmentAppearanceComponent(
    bool wornArmor,
    int headSlot,
    int bodySlot,
    int legSlot,
    bool social,
    bool vanity,
    bool newAndShiny,
    bool hasVanityEffects,
    sbyte handOnSlot = -1,
    sbyte handOffSlot = -1,
    sbyte backSlot = -1,
    sbyte frontSlot = -1,
    sbyte shoeSlot = -1,
    sbyte waistSlot = -1,
    sbyte wingSlot = -1,
    sbyte shieldSlot = -1,
    sbyte neckSlot = -1,
    sbyte faceSlot = -1,
    sbyte balloonSlot = -1,
    sbyte beardSlot = -1,
    sbyte voiceSlot = 0)
  {
    WornArmor = wornArmor;
    HeadSlot = headSlot;
    BodySlot = bodySlot;
    LegSlot = legSlot;
    Social = social;
    Vanity = vanity;
    NewAndShiny = newAndShiny;
    HasVanityEffects = hasVanityEffects;
    HandOnSlot = handOnSlot;
    HandOffSlot = handOffSlot;
    BackSlot = backSlot;
    FrontSlot = frontSlot;
    ShoeSlot = shoeSlot;
    WaistSlot = waistSlot;
    WingSlot = wingSlot;
    ShieldSlot = shieldSlot;
    NeckSlot = neckSlot;
    FaceSlot = faceSlot;
    BalloonSlot = balloonSlot;
    BeardSlot = beardSlot;
    VoiceSlot = voiceSlot;
  }

  public bool WornArmor { get; }

  public int HeadSlot { get; }

  public int BodySlot { get; }

  public int LegSlot { get; }

  public bool Social { get; }

  public bool Vanity { get; }

  public bool NewAndShiny { get; private set; }

  public bool HasVanityEffects { get; }

  public sbyte HandOnSlot { get; }

  public sbyte HandOffSlot { get; }

  public sbyte BackSlot { get; }

  public sbyte FrontSlot { get; }

  public sbyte ShoeSlot { get; }

  public sbyte WaistSlot { get; }

  public sbyte WingSlot { get; }

  public sbyte ShieldSlot { get; }

  public sbyte NeckSlot { get; }

  public sbyte FaceSlot { get; }

  public sbyte BalloonSlot { get; }

  public sbyte BeardSlot { get; }

  public sbyte VoiceSlot { get; }

  public void SetNewAndShiny(bool newAndShiny)
  {
    NewAndShiny = newAndShiny;
  }
}
