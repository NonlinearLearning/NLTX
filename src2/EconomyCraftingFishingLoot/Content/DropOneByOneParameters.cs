namespace NLTX.EconomyCraftingFishingLoot.Content;

public readonly record struct DropOneByOneParameters
{
  public DropOneByOneParameters(
    int chanceNumerator,
    int chanceDenominator,
    int minimumItemDropsCount,
    int maximumItemDropsCount,
    int minimumStackPerChunkBase,
    int maximumStackPerChunkBase,
    int bonusMinDropsPerChunkPerPlayer,
    int bonusMaxDropsPerChunkPerPlayer)
  {
    if (chanceDenominator <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(chanceDenominator));
    }

    if (chanceNumerator < 0 || chanceNumerator > chanceDenominator)
    {
      throw new ArgumentOutOfRangeException(nameof(chanceNumerator));
    }

    ValidateRange(
      minimumItemDropsCount,
      maximumItemDropsCount,
      nameof(minimumItemDropsCount),
      nameof(maximumItemDropsCount));
    ValidateRange(
      minimumStackPerChunkBase,
      maximumStackPerChunkBase,
      nameof(minimumStackPerChunkBase),
      nameof(maximumStackPerChunkBase));
    ValidateNonNegative(bonusMinDropsPerChunkPerPlayer, nameof(bonusMinDropsPerChunkPerPlayer));
    ValidateNonNegative(bonusMaxDropsPerChunkPerPlayer, nameof(bonusMaxDropsPerChunkPerPlayer));
    if (bonusMaxDropsPerChunkPerPlayer < bonusMinDropsPerChunkPerPlayer)
    {
      throw new ArgumentOutOfRangeException(nameof(bonusMaxDropsPerChunkPerPlayer));
    }

    ChanceNumerator = chanceNumerator;
    ChanceDenominator = chanceDenominator;
    MinimumItemDropsCount = minimumItemDropsCount;
    MaximumItemDropsCount = maximumItemDropsCount;
    MinimumStackPerChunkBase = minimumStackPerChunkBase;
    MaximumStackPerChunkBase = maximumStackPerChunkBase;
    BonusMinDropsPerChunkPerPlayer = bonusMinDropsPerChunkPerPlayer;
    BonusMaxDropsPerChunkPerPlayer = bonusMaxDropsPerChunkPerPlayer;
  }

  public int ChanceNumerator { get; }

  public int ChanceDenominator { get; }

  public int MinimumItemDropsCount { get; }

  public int MaximumItemDropsCount { get; }

  public int MinimumStackPerChunkBase { get; }

  public int MaximumStackPerChunkBase { get; }

  public int BonusMinDropsPerChunkPerPlayer { get; }

  public int BonusMaxDropsPerChunkPerPlayer { get; }

  public float PersonalDropRate =>
    (float)ChanceNumerator / ChanceDenominator;

  private static void ValidateRange(
    int minimum,
    int maximum,
    string minimumName,
    string maximumName)
  {
    ValidateNonNegative(minimum, minimumName);
    if (maximum < minimum)
    {
      throw new ArgumentOutOfRangeException(maximumName);
    }
  }

  private static void ValidateNonNegative(int value, string parameterName)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
