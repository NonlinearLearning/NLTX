namespace NLTX.EconomyCraftingFishingLoot.Content;

public sealed class ItemPickupTimingDefinition
{
  public ItemPickupTimingDefinition(
    int coinGrabRange,
    int manaGrabRange,
    int lifeGrabRange,
    int treasureGrabRange)
  {
    ValidateNonNegative(coinGrabRange, nameof(coinGrabRange));
    ValidateNonNegative(manaGrabRange, nameof(manaGrabRange));
    ValidateNonNegative(lifeGrabRange, nameof(lifeGrabRange));
    ValidateNonNegative(treasureGrabRange, nameof(treasureGrabRange));

    CoinGrabRange = coinGrabRange;
    ManaGrabRange = manaGrabRange;
    LifeGrabRange = lifeGrabRange;
    TreasureGrabRange = treasureGrabRange;
  }

  public int CoinGrabRange { get; }

  public int ManaGrabRange { get; }

  public int LifeGrabRange { get; }

  public int TreasureGrabRange { get; }

  private static void ValidateNonNegative(int value, string parameterName)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
