using System;

namespace Terraria.DeathPenaltyAndRevenge;

public sealed class RevengeMarkerValueComponent
{
  public RevengeMarkerValueComponent(float baseValue, int coinsValue)
  {
    if (!float.IsFinite(baseValue))
    {
      throw new ArgumentOutOfRangeException(
        nameof(baseValue),
        baseValue,
        "Base value must be finite.");
    }

    BaseValue = baseValue;
    CoinsValue = coinsValue;
  }

  public float BaseValue { get; }

  public int CoinsValue { get; }
}
