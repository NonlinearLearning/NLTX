namespace NLTX.EconomyCraftingFishingLoot.Fishing;

public sealed class PlayerFishingLevelQueryResult
{
  public PlayerFishingLevelQueryResult(
    PlayerFishingInputSnapshot input,
    float levelMultipliers,
    int finalFishingLevel)
  {
    ArgumentNullException.ThrowIfNull(input);
    if (!float.IsFinite(levelMultipliers))
    {
      throw new ArgumentOutOfRangeException(nameof(levelMultipliers));
    }

    if (finalFishingLevel < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(finalFishingLevel));
    }

    Input = input;
    LevelMultipliers = levelMultipliers;
    FinalFishingLevel = finalFishingLevel;
  }

  public PlayerFishingInputSnapshot Input { get; }

  public float LevelMultipliers { get; }

  public int FinalFishingLevel { get; }
}
