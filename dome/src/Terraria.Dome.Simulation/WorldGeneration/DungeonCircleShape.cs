using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly struct DungeonCircleShape
{
  public DungeonCircleShape(int horizontalRadius, int verticalRadius)
  {
    if (horizontalRadius < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(horizontalRadius));
    }

    if (verticalRadius <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(verticalRadius));
    }

    HorizontalRadius = horizontalRadius;
    VerticalRadius = verticalRadius;
  }

  public int HorizontalRadius { get; }

  public int VerticalRadius { get; }

  public int GetHorizontalExtent(int verticalOffset)
  {
    if (verticalOffset < -VerticalRadius || verticalOffset > VerticalRadius)
    {
      throw new ArgumentOutOfRangeException(nameof(verticalOffset));
    }

    double scaledOffset = (double)HorizontalRadius / VerticalRadius * verticalOffset;
    double squaredRadius = (double)(HorizontalRadius + 1) * (HorizontalRadius + 1);
    return Math.Min(
      HorizontalRadius,
      (int)Math.Sqrt(Math.Max(0.0, squaredRadius - scaledOffset * scaledOffset)));
  }

  public bool Contains(int horizontalOffset, int verticalOffset)
  {
    if (verticalOffset < -VerticalRadius || verticalOffset > VerticalRadius)
    {
      return false;
    }

    return Math.Abs(horizontalOffset) <= GetHorizontalExtent(verticalOffset);
  }
}
