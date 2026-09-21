namespace NLTX.EconomyCraftingFishingLoot.Fishing;

public sealed class FishingPowerSnapshot
{
  public FishingPowerSnapshot(int fishingLevel, bool canFishInLava)
  {
    if (fishingLevel < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(fishingLevel));
    }

    FishingLevel = fishingLevel;
    CanFishInLava = canFishInLava;
  }

  public int FishingLevel { get; }

  public bool CanFishInLava { get; }
}
