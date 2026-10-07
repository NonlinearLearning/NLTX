using System;

namespace Terraria.Player;

public readonly record struct PlayerContactImmunityComponent
{
  public PlayerContactImmunityComponent(int remainingTicks)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(remainingTicks);
    RemainingTicks = remainingTicks;
  }

  public int RemainingTicks { get; }
}
