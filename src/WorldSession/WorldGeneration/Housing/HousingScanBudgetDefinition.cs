using System;

namespace Terraria.WorldGeneration.Housing;

public readonly record struct HousingScanBudgetDefinition
{
  public HousingScanBudgetDefinition(int maxTileCount, int maxWallOut2)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxTileCount);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxWallOut2);

    MaxTileCount = maxTileCount;
    MaxWallOut2 = maxWallOut2;
  }

  public int MaxTileCount { get; }

  public int MaxWallOut2 { get; }
}
