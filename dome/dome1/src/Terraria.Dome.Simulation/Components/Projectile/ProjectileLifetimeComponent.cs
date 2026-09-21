using System;

namespace Terraria.Dome.Simulation.Components;

public struct ProjectileLifetimeComponent
{
  public ProjectileLifetimeComponent(int remainingTicks)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(remainingTicks);
    RemainingTicks = remainingTicks;
  }

  public int RemainingTicks;

  public int TimeLeft
  {
    get => RemainingTicks;
    set
    {
      ArgumentOutOfRangeException.ThrowIfNegative(value);
      RemainingTicks = value;
    }
  }

  public bool IsActive => RemainingTicks > 0;

  public bool IsExpired => RemainingTicks == 0;
}
