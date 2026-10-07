using System;

namespace Terraria.WorldGeneration.Queries;

public readonly record struct UndergroundDesertResetInput
{
  public UndergroundDesertResetInput(int worldWidth, int worldHeight)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(worldWidth);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(worldHeight);

    WorldWidth = worldWidth;
    WorldHeight = worldHeight;
  }

  public int WorldWidth { get; }

  public int WorldHeight { get; }
}
