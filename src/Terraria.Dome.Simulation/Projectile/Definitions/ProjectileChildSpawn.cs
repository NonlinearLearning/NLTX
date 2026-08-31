using System;

namespace Terraria.Dome.Simulation.Projectile.Definitions;

public readonly record struct ProjectileChildSpawn(
  int ProjectileType,
  int MinimumCount,
  int MaximumCount,
  float MinimumSpeed,
  float MaximumSpeed,
  float DamageMultiplier,
  float KnockbackMultiplier)
{
  public bool IsEnabled => ProjectileType != 0;

  public void Validate()
  {
    if ((ProjectileType == 0) != (MinimumCount == 0 && MaximumCount == 0) ||
        (ProjectileType != 0 &&
          (MinimumCount <= 0 || MaximumCount < MinimumCount ||
           !float.IsFinite(MinimumSpeed) || !float.IsFinite(MaximumSpeed) ||
           MinimumSpeed <= 0.0f || MaximumSpeed < MinimumSpeed ||
           !float.IsFinite(DamageMultiplier) || !float.IsFinite(KnockbackMultiplier) ||
           DamageMultiplier < 0.0f || KnockbackMultiplier < 0.0f)))
    {
      throw new ArgumentOutOfRangeException(nameof(ProjectileChildSpawn));
    }
  }
}
