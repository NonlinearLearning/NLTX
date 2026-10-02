namespace NLTX.EconomyCraftingFishingLoot.Fishing;

public static class PlayerFishingLevelQuery
{
  public static PlayerFishingLevelQueryResult Evaluate(
    PlayerFishingInputSnapshot input,
    float levelMultipliers,
    int finalFishingLevel)
  {
    ArgumentNullException.ThrowIfNull(input);
    return new PlayerFishingLevelQueryResult(
      input,
      levelMultipliers,
      finalFishingLevel);
  }
}
