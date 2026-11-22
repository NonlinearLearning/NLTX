using System;

namespace Terraria.Attributes;

/// <summary>Explicit periodic effect metadata shared by a status effect definition.</summary>
public readonly record struct BuffPeriodicEffect
{
  public BuffPeriodicEffect(BuffBehaviorKind kind, int periodTicks, int magnitude)
  {
    if (kind is not (BuffBehaviorKind.DamageOverTime or BuffBehaviorKind.PeriodicHeal))
    {
      throw new ArgumentException("Only periodic damage or healing can use this definition.", nameof(kind));
    }

    if (periodTicks <= 0 || magnitude <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(periodTicks));
    }

    Kind = kind;
    PeriodTicks = periodTicks;
    Magnitude = magnitude;
  }

  public BuffBehaviorKind Kind { get; }

  public int PeriodTicks { get; }

  public int Magnitude { get; }
}
