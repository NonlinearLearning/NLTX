using System;

namespace Terraria.DeathPenaltyAndRevenge;

public readonly record struct RevengeEnemyCaptureBounds
{
  public RevengeEnemyCaptureBounds(
    float left,
    float top,
    float right,
    float bottom)
  {
    EnsureFinite(left, nameof(left));
    EnsureFinite(top, nameof(top));
    EnsureFinite(right, nameof(right));
    EnsureFinite(bottom, nameof(bottom));

    if (right < left)
    {
      throw new ArgumentOutOfRangeException(nameof(right));
    }

    if (bottom < top)
    {
      throw new ArgumentOutOfRangeException(nameof(bottom));
    }

    Left = left;
    Top = top;
    Right = right;
    Bottom = bottom;
  }

  public float Left { get; }

  public float Top { get; }

  public float Right { get; }

  public float Bottom { get; }

  private static void EnsureFinite(float value, string parameterName)
  {
    if (!float.IsFinite(value))
    {
      throw new ArgumentOutOfRangeException(parameterName, value, "Value must be finite.");
    }
  }
}
