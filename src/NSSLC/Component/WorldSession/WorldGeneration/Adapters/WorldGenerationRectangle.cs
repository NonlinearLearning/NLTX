using System;

namespace Terraria.WorldGeneration.Adapters;

public readonly record struct WorldGenerationRectangle
{
  public WorldGenerationRectangle(int x, int y, int width, int height)
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

  public int Right => checked(X + Width);

  public int Bottom => checked(Y + Height);

  public bool Intersects(WorldGenerationRectangle other)
  {
    return (long)X < (long)other.X + other.Width &&
      (long)other.X < (long)X + Width &&
      (long)Y < (long)other.Y + other.Height &&
      (long)other.Y < (long)Y + Height;
  }
}
