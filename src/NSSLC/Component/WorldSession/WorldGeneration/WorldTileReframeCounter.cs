using System;

namespace Terraria.WorldGeneration.Components;

public sealed class WorldTileReframeCounter
{
  public int ActiveCount { get; private set; }

  public bool IsActive => ActiveCount > 0;

  public void Enter()
  {
    if (ActiveCount == int.MaxValue)
    {
      throw new InvalidOperationException(
        "The tile reframe count cannot exceed Int32.MaxValue.");
    }

    ActiveCount++;
  }

  public void Exit()
  {
    if (ActiveCount == 0)
    {
      throw new InvalidOperationException(
        "A tile reframe cannot exit without an active reframe.");
    }

    ActiveCount--;
  }

  public void Reset()
  {
    ActiveCount = 0;
  }
}
