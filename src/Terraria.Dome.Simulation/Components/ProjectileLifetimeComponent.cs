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
}
