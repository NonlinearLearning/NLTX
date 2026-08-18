using System;

namespace Terraria.Dome.Simulation.Combat.Components;

public struct HealthRegenerationComponent
{
  public const int DefaultDelayTicks = 2;

  public HealthRegenerationComponent(int delayTicks = DefaultDelayTicks)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(delayTicks);
    DelayTicks = delayTicks;
    RegenerationAccumulator = 0;
  }

  public int DelayTicks;
  public int RegenerationAccumulator;

  public void ResetDelay()
  {
    DelayTicks = DefaultDelayTicks;
    RegenerationAccumulator = 0;
  }
}
