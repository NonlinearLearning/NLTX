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

  public bool Intersects(WorldGenerationRectangle other)
  {
    return X < other.X + other.Width &&
      other.X < X + Width &&
      Y < other.Y + other.Height &&
      other.Y < Y + Height;
  }
}
