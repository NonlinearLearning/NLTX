namespace NLTX.EconomyCraftingFishingLoot.Content;

public readonly record struct FishingPossibilityEntry
{
  public FishingPossibilityEntry(int itemType, float frequency)
  {
    if (itemType < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(itemType));
    }

    if (float.IsNaN(frequency) || float.IsInfinity(frequency) || frequency < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(frequency));
    }

    ItemType = itemType;
    Frequency = frequency;
  }

  public int ItemType { get; }

  public float Frequency { get; }
}
