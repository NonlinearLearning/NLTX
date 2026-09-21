namespace Terraria.WorldGeneration.Support;

public sealed class ShapeFloodFillWorkState
{
  public ShapeFloodFillWorkState(int maximumActions = 100)
  {
    if (maximumActions < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumActions));
    }

    MaximumActions = maximumActions;
  }

  public int MaximumActions { get; }

  public int ConsumedActions { get; private set; }

  public int RemainingActions => MaximumActions - ConsumedActions;

  public bool IsExhausted => RemainingActions == 0;

  public bool TryConsumeAction()
  {
    if (IsExhausted)
    {
      return false;
    }

    ConsumedActions++;
    return true;
  }

  public void Reset()
  {
    ConsumedActions = 0;
  }
}
