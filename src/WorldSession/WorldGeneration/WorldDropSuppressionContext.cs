using System;

namespace Terraria.WorldGeneration.Components;

public sealed class WorldDropSuppressionContext
{
  public int ActiveDepth { get; private set; }

  public bool IsSuppressed => ActiveDepth > 0;

  public void Enter()
  {
    if (ActiveDepth == int.MaxValue)
    {
      throw new InvalidOperationException(
        "The drop suppression scope cannot exceed Int32.MaxValue nesting levels.");
    }

    ActiveDepth++;
  }

  public void Exit()
  {
    if (ActiveDepth == 0)
    {
      throw new InvalidOperationException(
        "A drop suppression scope cannot exit without an active scope.");
    }

    ActiveDepth--;
  }

  public void Reset()
  {
    ActiveDepth = 0;
  }
}
