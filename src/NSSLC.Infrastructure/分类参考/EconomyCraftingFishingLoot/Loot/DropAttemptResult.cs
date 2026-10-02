namespace NLTX.EconomyCraftingFishingLoot.Loot;

public sealed class DropAttemptResult
{
  private DropAttemptResult(
    DropAttemptResultState state,
    int itemId,
    int stackMinimum,
    int stackMaximum,
    int rollCount)
  {
    State = state;
    ItemId = itemId;
    StackMinimum = stackMinimum;
    StackMaximum = stackMaximum;
    RollCount = rollCount;
  }

  public DropAttemptResultState State { get; }

  public int ItemId { get; }

  public int StackMinimum { get; }

  public int StackMaximum { get; }

  public int RollCount { get; }

  public static DropAttemptResult Success(
    int itemId,
    int stackMinimum,
    int stackMaximum,
    int rollCount)
  {
    if (itemId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(itemId));
    }

    if (stackMinimum < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(stackMinimum));
    }

    if (stackMaximum < stackMinimum)
    {
      throw new ArgumentOutOfRangeException(nameof(stackMaximum));
    }

    if (rollCount <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(rollCount));
    }

    return new DropAttemptResult(
      DropAttemptResultState.Success,
      itemId,
      stackMinimum,
      stackMaximum,
      rollCount);
  }

  public static DropAttemptResult DoesntFillConditions()
  {
    return new DropAttemptResult(
      DropAttemptResultState.DoesntFillConditions,
      -1,
      0,
      0,
      0);
  }

  public static DropAttemptResult FailedRandomRoll(int rollCount)
  {
    if (rollCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(rollCount));
    }

    return new DropAttemptResult(
      DropAttemptResultState.FailedRandomRoll,
      -1,
      0,
      0,
      rollCount);
  }

  public static DropAttemptResult DidNotRunCode()
  {
    return new DropAttemptResult(
      DropAttemptResultState.DidNotRunCode,
      -1,
      0,
      0,
      0);
  }
}
