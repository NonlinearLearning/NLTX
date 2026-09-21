namespace NLTX.EconomyCraftingFishingLoot.Fishing;

public sealed class FishingEnvironmentSnapshot
{
  public FishingEnvironmentSnapshot(
    bool inLava,
    bool inHoney,
    int waterTilesCount,
    int waterNeededToFish,
    float waterQuality,
    int chumsInWater)
  {
    if (waterTilesCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(waterTilesCount));
    }

    if (waterNeededToFish < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(waterNeededToFish));
    }

    if (chumsInWater < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(chumsInWater));
    }

    if (!float.IsFinite(waterQuality))
    {
      throw new ArgumentOutOfRangeException(nameof(waterQuality));
    }

    InLava = inLava;
    InHoney = inHoney;
    WaterTilesCount = waterTilesCount;
    WaterNeededToFish = waterNeededToFish;
    WaterQuality = waterQuality;
    ChumsInWater = chumsInWater;
  }

  public bool InLava { get; }

  public bool InHoney { get; }

  public int WaterTilesCount { get; }

  public int WaterNeededToFish { get; }

  public float WaterQuality { get; }

  public int ChumsInWater { get; }
}
