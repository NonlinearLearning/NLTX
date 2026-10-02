using NLTX.EconomyCraftingFishingLoot.Loot;

namespace NLTX.EconomyCraftingFishingLoot.Fishing;

public sealed class FishingCatchAttributionAdapter
{
  public DropSourceAttribution Adapt(string sourceEntityKey)
  {
    return DropSourceAttribution.FishedOut(sourceEntityKey);
  }
}
