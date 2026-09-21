namespace NLTX.EconomyCraftingFishingLoot.Content;

public sealed class ItemStackAndUseDelayDefinition
{
  public ItemStackAndUseDelayDefinition(
    int commonMaxStack,
    int potionDelay,
    int restorationDelay,
    int eggnogDelay,
    int mushroomDelay)
  {
    ValidatePositive(commonMaxStack, nameof(commonMaxStack));
    ValidateNonNegative(potionDelay, nameof(potionDelay));
    ValidateNonNegative(restorationDelay, nameof(restorationDelay));
    ValidateNonNegative(eggnogDelay, nameof(eggnogDelay));
    ValidateNonNegative(mushroomDelay, nameof(mushroomDelay));

    CommonMaxStack = commonMaxStack;
    PotionDelay = potionDelay;
    RestorationDelay = restorationDelay;
    EggnogDelay = eggnogDelay;
    MushroomDelay = mushroomDelay;
  }

  public int CommonMaxStack { get; }

  public int PotionDelay { get; }

  public int RestorationDelay { get; }

  public int EggnogDelay { get; }

  public int MushroomDelay { get; }

  private static void ValidateNonNegative(int value, string parameterName)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }

  private static void ValidatePositive(int value, string parameterName)
  {
    if (value <= 0)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
