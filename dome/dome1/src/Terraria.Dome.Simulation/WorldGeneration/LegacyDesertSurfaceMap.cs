using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed class LegacyDesertSurfaceMap
{
  private readonly short[] _heights;

  private LegacyDesertSurfaceMap(short[] heights, int x, double worldSurface)
  {
    _heights = heights;
    X = x;
    int maximum = 0;
    int minimum = int.MaxValue;
    int sum = 0;
    for (int index = 0; index < heights.Length; index++)
    {
      short height = heights[index];
      sum += height;
      maximum = Math.Max(maximum, height);
      minimum = Math.Min(minimum, height);
    }

    if (maximum > worldSurface - 10.0)
    {
      maximum = (int)worldSurface - 10;
    }

    Bottom = maximum;
    Top = minimum;
    Average = (double)sum / heights.Length;
  }

  public double Average { get; }

  public int Bottom { get; }

  public int Top { get; }

  public int Width => _heights.Length;

  public int X { get; }

  public short this[int absoluteX]
  {
    get
    {
      int index = absoluteX - X;
      if ((uint)index >= (uint)_heights.Length)
      {
        throw new ArgumentOutOfRangeException(nameof(absoluteX));
      }

      return _heights[index];
    }
  }

  public static LegacyDesertSurfaceMap FromHeights(
    IReadOnlyList<short> heights,
    int x,
    double worldSurface)
  {
    ArgumentNullException.ThrowIfNull(heights);
    if (heights.Count == 0)
    {
      throw new ArgumentException("A desert surface map requires at least one height.", nameof(heights));
    }

    if (double.IsNaN(worldSurface) || double.IsInfinity(worldSurface))
    {
      throw new ArgumentOutOfRangeException(nameof(worldSurface));
    }

    short[] copiedHeights = new short[heights.Count];
    for (int index = 0; index < heights.Count; index++)
    {
      copiedHeights[index] = heights[index];
    }

    return new LegacyDesertSurfaceMap(copiedHeights, x, worldSurface);
  }
}
