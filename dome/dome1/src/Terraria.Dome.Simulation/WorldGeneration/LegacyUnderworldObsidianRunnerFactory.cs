using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyUnderworldObsidianRunnerFactory
{
  public const double Density = 0.0008;

  public static int CalculateCount(int width, int height)
  {
    if (width <= 0 || height <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    return checked((int)(width * (double)height * Density));
  }

  public static LegacyTileRunnerRequest Create(int width, int height, LegacyPassRandomState random)
  {
    ArgumentNullException.ThrowIfNull(random);
    if (width <= 0 || height <= 140)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    return new LegacyTileRunnerRequest(
      random.Next(width),
      random.Next(height - 140, height),
      random.Next(2, 7),
      random.Next(3, 7),
      58,
      addTile: false,
      speedX: 0.0,
      speedY: 0.0,
      noYChange: false,
      overwrite: false,
      ignoreTileType: -1);
  }
}
