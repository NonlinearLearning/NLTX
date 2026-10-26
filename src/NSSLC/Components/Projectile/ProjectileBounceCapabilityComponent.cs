using System;

namespace Terraria.Projectile;

/// <summary>Declares that a projectile may reflect from accepted collision contacts.</summary>
public readonly record struct ProjectileBounceCapabilityComponent
{
  public ProjectileBounceCapabilityComponent(int maximumBounces, bool enabled = true)
  {
    if (maximumBounces < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumBounces));
    }

    MaximumBounces = maximumBounces;
    Enabled = enabled;
  }

  public int MaximumBounces { get; }

  public bool Enabled { get; }
}
