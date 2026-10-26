using System;

namespace Terraria.Projectile;

/// <summary>Controls bounded projectile substeps without duplicating spatial velocity.</summary>
public readonly record struct ProjectileKinematicsStateComponent
{
  public const int MaximumExtraUpdates = 1024;

  public ProjectileKinematicsStateComponent(int extraUpdates, int substep = 0)
  {
    if (extraUpdates < 0 || extraUpdates > MaximumExtraUpdates)
    {
      throw new ArgumentOutOfRangeException(nameof(extraUpdates));
    }

    if (substep < 0 || substep > extraUpdates)
    {
      throw new ArgumentOutOfRangeException(nameof(substep));
    }

    ExtraUpdates = extraUpdates;
    Substep = substep;
  }

  public int ExtraUpdates { get; }

  public int Substep { get; }

  public int SubstepCount => ExtraUpdates + 1;

  public ProjectileKinematicsStateComponent BeginTick()
  {
    return new ProjectileKinematicsStateComponent(ExtraUpdates, 0);
  }

  public ProjectileKinematicsStateComponent NextSubstep()
  {
    int nextSubstep = Math.Min(Substep + 1, ExtraUpdates);
    return new ProjectileKinematicsStateComponent(ExtraUpdates, nextSubstep);
  }
}
