namespace NLTX.EconomyCraftingFishingLoot.Commerce;

public static class ShopPricePolicyDefinition
{
  public const float LowestPossiblePriceMultiplier = 0.75f;

  public const float MaxHappinessAchievementPriceMultiplier = 0.82f;

  public const float HighestPossiblePriceMultiplier = 1.5f;

  public static float ClampPriceAdjustment(float priceAdjustment)
  {
    if (!float.IsFinite(priceAdjustment))
    {
      throw new ArgumentOutOfRangeException(nameof(priceAdjustment));
    }

    return Math.Clamp(
      priceAdjustment,
      LowestPossiblePriceMultiplier,
      HighestPossiblePriceMultiplier);
  }
}
