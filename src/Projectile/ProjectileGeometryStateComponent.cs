using System;

namespace Terraria.Projectile;

public struct ProjectileGeometryStateComponent
{
  public ProjectileGeometryStateComponent(
    float scale = 1.0f,
    bool reflected = false)
  {
    if (!float.IsFinite(scale) || scale < 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(scale));
    }

    Scale = scale;
    Reflected = reflected;
  }

  public float Scale;
  public bool Reflected;
}
