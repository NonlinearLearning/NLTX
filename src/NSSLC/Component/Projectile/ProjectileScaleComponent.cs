using System;

namespace Terraria.Projectile;

/// <summary>The projectile's non-geometric scale value.</summary>
public readonly record struct ProjectileScaleComponent
{
  public ProjectileScaleComponent(float scale)
  {
    if (!float.IsFinite(scale) || scale < 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(scale));
    }

    Scale = scale;
  }

  public float Scale { get; }
}
