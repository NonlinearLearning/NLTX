namespace NLTX.EconomyCraftingFishingLoot.Content;

public sealed class ItemBuffDurationDefinition
{
  public ItemBuffDurationDefinition(
    int luckPotionDuration1,
    int luckPotionDuration2,
    int luckPotionDuration3,
    int flaskTime)
  {
    ValidateNonNegative(luckPotionDuration1, nameof(luckPotionDuration1));
    ValidateNonNegative(luckPotionDuration2, nameof(luckPotionDuration2));
    ValidateNonNegative(luckPotionDuration3, nameof(luckPotionDuration3));
    ValidateNonNegative(flaskTime, nameof(flaskTime));

    LuckPotionDuration1 = luckPotionDuration1;
    LuckPotionDuration2 = luckPotionDuration2;
    LuckPotionDuration3 = luckPotionDuration3;
    FlaskTime = flaskTime;
  }

  public int LuckPotionDuration1 { get; }

  public int LuckPotionDuration2 { get; }

  public int LuckPotionDuration3 { get; }

  public int FlaskTime { get; }

  private static void ValidateNonNegative(int value, string parameterName)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
