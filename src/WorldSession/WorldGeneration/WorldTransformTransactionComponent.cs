using System;

namespace Terraria.WorldGeneration.Components;

public sealed class WorldTransformTransactionComponent
{
  public int ActiveCount { get; private set; }

  public ulong Revision { get; private set; }

  public bool IsTransforming => ActiveCount > 0;

  public void Begin()
  {
    if (ActiveCount == int.MaxValue)
    {
      throw new InvalidOperationException(
        "The active world transform count cannot exceed Int32.MaxValue.");
    }

    ActiveCount++;
    Revision++;
  }

  public void End()
  {
    if (ActiveCount == 0)
    {
      throw new InvalidOperationException(
        "A world transform cannot complete without an active transaction.");
    }

    ActiveCount--;
  }

  public void Reset()
  {
    ActiveCount = 0;
  }
}
