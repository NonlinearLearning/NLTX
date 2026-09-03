using System;

namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcPixelRectangle
{
  public NpcPixelRectangle(int x, int y, int width, int height)
  {
    if (width <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    if (height <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(height));
    }

    X = x;
    Y = y;
    Width = width;
    Height = height;
  }

  public int X { get; }

  public int Y { get; }

  public int Width { get; }

  public int Height { get; }

  public bool Intersects(NpcPixelRectangle other)
  {
    Validate();
    other.Validate();
    return HasStrictOverlap(X, Width, other.X, other.Width) &&
      HasStrictOverlap(Y, Height, other.Y, other.Height);
  }

  internal void Validate()
  {
    if (Width <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(Width));
    }

    if (Height <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(Height));
    }
  }

  private static bool HasStrictOverlap(
    int firstStart,
    int firstLength,
    int secondStart,
    int secondLength)
  {
    long firstEnd = (long)firstStart + firstLength;
    long secondEnd = (long)secondStart + secondLength;
    return firstStart < secondEnd && secondStart < firstEnd;
  }
}
