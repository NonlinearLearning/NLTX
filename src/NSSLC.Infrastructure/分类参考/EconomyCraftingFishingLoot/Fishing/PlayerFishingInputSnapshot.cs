namespace NLTX.EconomyCraftingFishingLoot.Fishing;

public sealed class PlayerFishingInputSnapshot
{
  public PlayerFishingInputSnapshot(
    int polePower,
    int poleItemType,
    int baitPower,
    int baitItemType)
  {
    if (polePower < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(polePower));
    }

    if (baitPower < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(baitPower));
    }

    if (poleItemType < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(poleItemType));
    }

    if (baitItemType < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(baitItemType));
    }

    PolePower = polePower;
    PoleItemType = poleItemType;
    BaitPower = baitPower;
    BaitItemType = baitItemType;
  }

  public int PolePower { get; }

  public int PoleItemType { get; }

  public int BaitPower { get; }

  public int BaitItemType { get; }
}
