namespace NLTX.EconomyCraftingFishingLoot.Commerce;

public readonly record struct ShoppingSettingsProjection
{
  public ShoppingSettingsProjection(float priceAdjustment, string happinessReport)
  {
    if (!float.IsFinite(priceAdjustment))
    {
      throw new ArgumentOutOfRangeException(nameof(priceAdjustment));
    }

    PriceAdjustment = priceAdjustment;
    HappinessReport = happinessReport ?? string.Empty;
  }

  public float PriceAdjustment { get; }

  public string HappinessReport { get; }

  public static ShoppingSettingsProjection NotInShop =>
    new(1f, string.Empty);
}
