using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyUnderworldLowerEvilRunnerFactory
{
  public static LegacyTileRunnerRequest Create(
    int width,
    int height,
    LegacyPassRandomState random)
  {
    ArgumentNullException.ThrowIfNull(random);
    if (width <= 40 || height <= 180)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    return new LegacyTileRunnerRequest(
      random.Next(20, width - 20),
      random.Next(height - 180, height - 10),
      random.Next(2, 7),
      random.Next(2, 7),
      -2,
      addTile: false,
      speedX: 0.0,
      speedY: 0.0,
      noYChange: false,
      overwrite: false,
      ignoreTileType: -1);
  }
}
