using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyQuickWaterScanPolicy
{
  public const int MinimumY = 3;
  public const int MaximumXInset = 4;

  public static (int MinimumYInclusive, int MaximumYInclusive) GetYRange(int height)
  {
    if (height <= MinimumY * 2)
    {
      throw new ArgumentOutOfRangeException(nameof(height));
    }

    return (MinimumY, height - MinimumY);
  }

  public static IEnumerable<(int X, int Y)> EnumerateScan(int width, int height)
  {
    if (width <= MaximumXInset * 2 || height <= MinimumY * 2)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    (int minimumY, int maximumY) = GetYRange(height);
    for (int y = maximumY; y >= minimumY; y--)
    {
      for (int x = MaximumXInset; x < width - MaximumXInset; x++)
      {
        yield return (x, y);
      }
    }
  }
}
