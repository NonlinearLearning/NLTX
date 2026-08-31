using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyUnderworldLavaColumnPolicy
{
  public static int CreateInitialTop(int height, LegacyPassRandomState random)
  {
    ArgumentNullException.ThrowIfNull(random);
    if (height <= 70)
    {
      throw new ArgumentOutOfRangeException(nameof(height));
    }

    return height - random.Next(40, 70);
  }

  public static int AdvanceTop(int previousTopY, int height, LegacyPassRandomState random)
  {
    ArgumentNullException.ThrowIfNull(random);
    if (previousTopY < 0 || previousTopY >= height || height <= 120)
    {
      throw new ArgumentOutOfRangeException(nameof(previousTopY));
    }

    int topY = previousTopY + random.Next(-10, 11);
    if (topY > height - 60)
    {
      topY = height - 60;
    }

    if (topY < height - 100)
    {
      topY = height - 120;
    }

    return topY;
  }

  public static bool ShouldFillLava(WorldTile tile)
  {
    return !tile.IsActive;
  }
}
