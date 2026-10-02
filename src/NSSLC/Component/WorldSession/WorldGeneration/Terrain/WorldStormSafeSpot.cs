using System;

namespace Terraria.WorldGeneration.Terrain;

public readonly record struct WorldStormSafeSpot
{
  public WorldStormSafeSpot(int x, int y, int width, int height)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(width);
    ArgumentOutOfRangeException.ThrowIfNegative(height);

    X = x;
    Y = y;
    Width = width;
    Height = height;
  }

  public int X { get; }

  public int Y { get; }

  public int Width { get; }

  public int Height { get; }

  public bool Contains(int x, int y)
  {
    return (long)x >= X &&
      (long)y >= Y &&
      (long)x < (long)X + Width &&
      (long)y < (long)Y + Height;
  }
}
