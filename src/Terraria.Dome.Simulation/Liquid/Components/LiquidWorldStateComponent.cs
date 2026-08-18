using System;

namespace Terraria.Dome.Simulation.Liquid.Components;

public sealed class LiquidWorldStateComponent
{
  public LiquidWorldStateComponent(int maximumQueueLength, int tickBudget)
  {
    if (maximumQueueLength <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumQueueLength));
    }

    if (tickBudget <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tickBudget));
    }

    MaximumQueueLength = maximumQueueLength;
    TickBudget = tickBudget;
  }

  public int MaximumQueueLength { get; }
  public LiquidSimulationMode Mode { get; private set; }
  public int TickBudget { get; }

  public void SetMode(LiquidSimulationMode mode)
  {
    if (!Enum.IsDefined(mode))
    {
      throw new ArgumentOutOfRangeException(nameof(mode));
    }

    Mode = mode;
  }
}
