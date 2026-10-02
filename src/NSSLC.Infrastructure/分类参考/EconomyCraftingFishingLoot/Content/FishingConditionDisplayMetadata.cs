namespace NLTX.EconomyCraftingFishingLoot.Content;

public sealed class FishingConditionDisplayMetadata
{
  public FishingConditionDisplayMetadata(bool canBeSkippedForDisplay)
  {
    CanBeSkippedForDisplay = canBeSkippedForDisplay;
  }

  public bool CanBeSkippedForDisplay { get; }
}
