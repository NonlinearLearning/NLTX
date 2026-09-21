using System;

namespace Terraria.WorldGeneration.Terrain;

public sealed class WorldGrassSpreadState
{
  public const int MaximumRecursiveSpreadDepth = 1000;

  public bool IsUndergroundPhase { get; private set; }

  public int RecursiveSpreadDepth { get; private set; }

  public void BeginUndergroundPhase()
  {
    IsUndergroundPhase = true;
  }

  public void EndUndergroundPhase()
  {
    IsUndergroundPhase = false;
  }

  public bool TryEnterRecursiveSpread()
  {
    if (RecursiveSpreadDepth >= MaximumRecursiveSpreadDepth)
    {
      return false;
    }

    RecursiveSpreadDepth++;
    return true;
  }

  public void ExitRecursiveSpread()
  {
    if (RecursiveSpreadDepth == 0)
    {
      throw new InvalidOperationException(
        "A grass spread scope cannot exit without a matching entry.");
    }

    RecursiveSpreadDepth--;
  }

  public void Reset()
  {
    IsUndergroundPhase = false;
    RecursiveSpreadDepth = 0;
  }
}
