using System;

namespace Terraria.Dome.Simulation.Projectile.Definitions;

public readonly record struct ProjectileOnHitStatusEffect(
  ushort Type,
  int MinimumDurationTicks,
  int MaximumDurationTicks,
  int ChanceNumerator = 1,
  int ChanceDenominator = 2)
{
  public bool IsEnabled => Type != 0 && MinimumDurationTicks > 0 &&
    MaximumDurationTicks >= MinimumDurationTicks;

  public void Validate()
  {
    if (!IsEnabled || ChanceNumerator <= 0 || ChanceDenominator <= 0 ||
        ChanceNumerator > ChanceDenominator)
    {
      throw new ArgumentOutOfRangeException(nameof(Type));
    }
  }
}
