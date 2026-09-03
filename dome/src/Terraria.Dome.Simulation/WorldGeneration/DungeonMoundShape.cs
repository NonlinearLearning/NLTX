using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly struct DungeonMoundShape
{
  public DungeonMoundShape(int halfWidth, int height)
  {
    if (halfWidth <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(halfWidth));
    }

    if (height < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(height));
    }

    HalfWidth = halfWidth;
    Height = height;
  }

  public int HalfWidth { get; }

  public int Height { get; }

  public int GetColumnHeight(int horizontalOffset)
  {
    if (horizontalOffset < -HalfWidth || horizontalOffset > HalfWidth)
    {
      throw new ArgumentOutOfRangeException(nameof(horizontalOffset));
    }

    double width = HalfWidth;
    double value = -((Height + 1.0) / (width * width)) *
      (horizontalOffset + width) * (horizontalOffset - width);
    return Math.Min(Height, Math.Max(0, (int)value));
  }

  public bool Contains(int horizontalOffset, int verticalOffset)
  {
    if (horizontalOffset < -HalfWidth || horizontalOffset > HalfWidth)
    {
      return false;
    }

    int columnHeight = GetColumnHeight(horizontalOffset);
    int startOffset = -(Height / 2);
    return verticalOffset >= startOffset && verticalOffset < startOffset + columnHeight;
  }
}
