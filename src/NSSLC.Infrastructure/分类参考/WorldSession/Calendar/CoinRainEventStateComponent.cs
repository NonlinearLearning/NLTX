using System;

namespace Terraria.WorldSession.Calendar;

public sealed class CoinRainEventStateComponent
{
  public CoinRainEventStateComponent(int remainingValue = 0)
  {
    RemainingValue = remainingValue;
    Validate();
  }

  public int RemainingValue { get; internal set; }

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(RemainingValue);
  }
}
