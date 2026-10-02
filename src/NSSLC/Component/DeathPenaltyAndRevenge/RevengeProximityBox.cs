using System;

using Terraria.Items;

namespace Terraria.DeathPenaltyAndRevenge;

public readonly record struct RevengeProximityBox
{
  public RevengeProximityBox(float left, float top, float width, float height)
  {
    EnsureFinite(left, nameof(left));
    EnsureFinite(top, nameof(top));
    EnsureFinite(width, nameof(width));
    EnsureFinite(height, nameof(height));

    if (width < 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    if (height < 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(height));
    }

    Left = left;
    Top = top;
    Width = width;
    Height = height;
  }

  public float Left { get; }
  public float Top { get; }
  public float Width { get; }
  public float Height { get; }
  public float Right => Left + Width;
  public float Bottom => Top + Height;

  public static RevengeProximityBox CenteredAt(
    WorldPosition center,
    float width,
    float height)
  {
    return new RevengeProximityBox(
      center.X - width * 0.5f,
      center.Y - height * 0.5f,
      width,
      height);
  }

  public bool Intersects(RevengeProximityBox other)
  {
    return Left < other.Right &&
      Top < other.Bottom &&
      Right > other.Left &&
      Bottom > other.Top;
  }

  private static void EnsureFinite(float value, string parameterName)
  {
    if (!float.IsFinite(value))
    {
      throw new ArgumentOutOfRangeException(parameterName, value, "Value must be finite.");
    }
  }
}
