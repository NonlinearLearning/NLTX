namespace NLTX.EconomyCraftingFishingLoot.Content;

public sealed class CommonDropChanceQuantityDefinition
{
  public CommonDropChanceQuantityDefinition(
    int itemId,
    int chanceDenominator,
    int amountDroppedMinimum = 1,
    int amountDroppedMaximum = 1,
    int chanceNumerator = 1)
  {
    if (itemId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(itemId));
    }

    if (chanceDenominator <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(chanceDenominator));
    }

    if (chanceNumerator < 0 || chanceNumerator > chanceDenominator)
    {
      throw new ArgumentOutOfRangeException(nameof(chanceNumerator));
    }

    if (amountDroppedMinimum < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(amountDroppedMinimum));
    }

    if (amountDroppedMaximum < amountDroppedMinimum)
    {
      throw new ArgumentOutOfRangeException(nameof(amountDroppedMaximum));
    }

    ItemId = itemId;
    ChanceDenominator = chanceDenominator;
    AmountDroppedMinimum = amountDroppedMinimum;
    AmountDroppedMaximum = amountDroppedMaximum;
    ChanceNumerator = chanceNumerator;
  }

  public int ItemId { get; }

  public int ChanceDenominator { get; }

  public int AmountDroppedMinimum { get; }

  public int AmountDroppedMaximum { get; }

  public int ChanceNumerator { get; }

  public float PersonalDropRate =>
    (float)ChanceNumerator / ChanceDenominator;
}
