namespace NLTX.EconomyCraftingFishingLoot.Content;

public sealed class ItemCurrencyEconomyDefinition
{
  public ItemCurrencyEconomyDefinition(
    int copper,
    int silver,
    int gold,
    int platinum,
    int goldCritterRarityColor)
  {
    ValidateNonNegative(copper, nameof(copper));
    ValidateNonNegative(silver, nameof(silver));
    ValidateNonNegative(gold, nameof(gold));
    ValidateNonNegative(platinum, nameof(platinum));
    ValidateNonNegative(goldCritterRarityColor, nameof(goldCritterRarityColor));

    Copper = copper;
    Silver = silver;
    Gold = gold;
    Platinum = platinum;
    GoldCritterRarityColor = goldCritterRarityColor;
  }

  public int Copper { get; }

  public int Silver { get; }

  public int Gold { get; }

  public int Platinum { get; }

  public int GoldCritterRarityColor { get; }

  private static void ValidateNonNegative(int value, string parameterName)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
