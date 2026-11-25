using System;

namespace Terraria.Share.Attributes;

/// <summary>Declares one non-numeric behavior produced by a status effect.</summary>
public readonly record struct BuffBehaviorContribution
{
  public BuffBehaviorContribution(
      BuffBehaviorKind kind,
      int periodTicks = 0,
      int magnitude = 0)
  {
    if (!Enum.IsDefined(kind))
    {
      throw new ArgumentOutOfRangeException(nameof(kind));
    }

    if (periodTicks < 0 || magnitude < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(periodTicks));
    }

    if (kind is BuffBehaviorKind.DamageOverTime or BuffBehaviorKind.PeriodicHeal &&
        (periodTicks <= 0 || magnitude <= 0))
    {
      throw new ArgumentException(
          "Periodic behavior requires a positive period and magnitude.", nameof(periodTicks));
    }

    Kind = kind;
    PeriodTicks = periodTicks;
    Magnitude = magnitude;
  }

  public BuffBehaviorKind Kind { get; }

  public int PeriodTicks { get; }

  public int Magnitude { get; }
}
