using System;

namespace Terraria.Unshared.Projectiles;

/// <summary>Declares that a projectile may compute a homing adjustment.</summary>
public readonly record struct ProjectileHomingCapabilityComponent
{
  public ProjectileHomingCapabilityComponent(
      float turnRate,
      float range,
      bool enabled = true)
  {
    if (!float.IsFinite(turnRate) || turnRate < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(turnRate));
    }

    if (!float.IsFinite(range) || range < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(range));
    }

    TurnRate = turnRate;
    Range = range;
    Enabled = enabled;
  }

  public float TurnRate { get; }

  public float Range { get; }

  public bool Enabled { get; }
}
