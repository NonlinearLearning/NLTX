namespace NLTX.EconomyCraftingFishingLoot.Fishing;

public sealed class FishingWorldPredicateSnapshot
{
  public FishingWorldPredicateSnapshot(
    float atmosphericValue,
    int fishingY,
    int heightLevel,
    double? rockLayerY = null,
    bool? isOriginalOcean = null)
  {
    if (!float.IsFinite(atmosphericValue))
    {
      throw new ArgumentOutOfRangeException(nameof(atmosphericValue));
    }

    if (rockLayerY is double layer && !double.IsFinite(layer))
    {
      throw new ArgumentOutOfRangeException(nameof(rockLayerY));
    }

    AtmosphericValue = atmosphericValue;
    FishingY = fishingY;
    HeightLevel = heightLevel;
    RockLayerY = rockLayerY;
    IsOriginalOcean = isOriginalOcean;
  }

  public float AtmosphericValue { get; }

  public int FishingY { get; }

  public int HeightLevel { get; }

  public double? RockLayerY { get; }

  public bool? IsOriginalOcean { get; }
}
