using System;

namespace Terraria.Dome.Simulation.Components;

public struct ProjectileBounceComponent
{
  public ProjectileBounceComponent(int remainingBounces)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(remainingBounces);
    RemainingBounces = remainingBounces;
  }

  public int RemainingBounces { get; set; }
}
