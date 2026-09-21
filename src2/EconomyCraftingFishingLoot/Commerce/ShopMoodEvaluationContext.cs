namespace NLTX.EconomyCraftingFishingLoot.Commerce;

public sealed class ShopMoodEvaluationContext
{
  public ShopMoodEvaluationContext(
    string buyerKey,
    string sellerKey,
    float currentPriceAdjustment = 1f,
    string currentHappiness = "")
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(buyerKey);
    ArgumentException.ThrowIfNullOrWhiteSpace(sellerKey);
    if (!float.IsFinite(currentPriceAdjustment))
    {
      throw new ArgumentOutOfRangeException(nameof(currentPriceAdjustment));
    }

    BuyerKey = buyerKey;
    SellerKey = sellerKey;
    CurrentPriceAdjustment = currentPriceAdjustment;
    CurrentHappiness = currentHappiness ?? string.Empty;
  }

  public string BuyerKey { get; }

  public string SellerKey { get; }

  public float CurrentPriceAdjustment { get; }

  public string CurrentHappiness { get; }
}
