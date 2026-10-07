using System;

namespace Terraria.Projectile;

public struct ProjectileGeometryStateComponent
{
  public ProjectileGeometryStateComponent(
    float scale = 1.0f)
    : this(0, 0, scale)
  {
  }

  public ProjectileGeometryStateComponent(
    int width,
    int height,
    float scale = 1.0f)
  {
    if (width < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    if (height < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(height));
    }

    if (!float.IsFinite(scale) || scale < 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(scale));
    }

    Width = width;
    Height = height;
    Scale = scale;
  }

  public int Width;
  public int Height;
  public float Scale;
}
