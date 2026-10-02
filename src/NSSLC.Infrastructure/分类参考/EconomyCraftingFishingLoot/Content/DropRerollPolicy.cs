namespace NLTX.EconomyCraftingFishingLoot.Content;

public sealed class DropRerollPolicy
{
  public DropRerollPolicy(int timesToRoll)
  {
    if (timesToRoll < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(timesToRoll));
    }

    TimesToRoll = timesToRoll;
    TotalRolls = checked(timesToRoll + 1);
  }

  public int TimesToRoll { get; }

  public int TotalRolls { get; }
}
