using System;

namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcPixelBounds
{
  public NpcPixelBounds(float x, float y, int width, int height)
  {
    if (!float.IsFinite(x))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    if (!float.IsFinite(y))
    {
      throw new ArgumentOutOfRangeException(nameof(y));
    }

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

  public float X { get; }

  public float Y { get; }

  public int Width { get; }

  public int Height { get; }

  internal void Validate()
  {
    if (!float.IsFinite(X))
    {
      throw new ArgumentOutOfRangeException(nameof(X));
    }

    if (!float.IsFinite(Y))
    {
      throw new ArgumentOutOfRangeException(nameof(Y));
    }

    if (Width <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(Width));
    }

    if (Height <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(Height));
    }
  }
}
