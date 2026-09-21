using System;

namespace Terraria.WorldGeneration.Definitions;

public readonly record struct WorldSkyblockGenerationDimensions
{
  public WorldSkyblockGenerationDimensions(int maxTilesX, int maxTilesY)
  {
    if (maxTilesX < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxTilesX));
    }

    if (maxTilesY < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxTilesY));
    }

    MaxTilesX = maxTilesX;
    MaxTilesY = maxTilesY;
  }

  public int MaxTilesX { get; }

  public int MaxTilesY { get; }

  public long WorldTileCount => (long)MaxTilesX * MaxTilesY;
}
