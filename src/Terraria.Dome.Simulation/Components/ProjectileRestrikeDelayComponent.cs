using System;

namespace Terraria.Dome.Simulation.Components;

public readonly record struct ProjectileRestrikeDelayComponent
{
  public ProjectileRestrikeDelayComponent(int remainingTicks = 0)
  {
    if (remainingTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(remainingTicks));
    }

    RemainingTicks = remainingTicks;
  }

  public int RemainingTicks { get; }
}
