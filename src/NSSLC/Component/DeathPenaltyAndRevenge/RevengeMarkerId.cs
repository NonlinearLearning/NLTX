using System;

namespace Terraria.DeathPenaltyAndRevenge;

public readonly record struct RevengeMarkerId
{
  public const int UnassignedValue = -1;

  public RevengeMarkerId(int value)
  {
    if (value < UnassignedValue)
    {
      throw new ArgumentOutOfRangeException(nameof(value));
    }

    Value = value;
  }

  public int Value { get; }

  public static RevengeMarkerId Unassigned => new(UnassignedValue);

  public bool IsAssigned => Value >= 0;
}
