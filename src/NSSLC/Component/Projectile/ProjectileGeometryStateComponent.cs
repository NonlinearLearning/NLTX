using System;

namespace Terraria.Projectile;

public struct ProjectileGeometryStateComponent
{
  public ProjectileGeometryStateComponent(
    float scale = 1.0f,
    bool reflected = false)
    : this(0, 0, scale, reflected)
  {
  }

  public ProjectileGeometryStateComponent(
    int width,
    int height,
    float scale = 1.0f,
    bool reflected = false)
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
    Reflected = reflected;
  }

  public int Width;
  public int Height;
  public float Scale;
  public bool Reflected;
}
