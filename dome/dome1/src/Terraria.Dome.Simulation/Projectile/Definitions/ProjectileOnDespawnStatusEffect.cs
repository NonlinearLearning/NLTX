using System;

namespace Terraria.Dome.Simulation.Projectile.Definitions;

public readonly record struct ProjectileOnDespawnStatusEffect(
  ushort Type,
  int DurationTicks,
  float Radius)
{
  public bool IsEnabled => Type != 0 && DurationTicks > 0 && Radius > 0.0f;

  public void Validate()
  {
    if ((!IsEnabled && (Type != 0 || DurationTicks != 0 || Radius != 0.0f)) ||
        !float.IsFinite(Radius))
    {
      throw new ArgumentOutOfRangeException(nameof(Radius));
    }
  }
}
