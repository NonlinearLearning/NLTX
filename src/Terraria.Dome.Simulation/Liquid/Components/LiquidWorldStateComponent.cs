using System;

namespace Terraria.Dome.Simulation.Liquid.Components;

public sealed class LiquidWorldStateComponent
{
  private int _sustainedHighWaterTicks;

  public LiquidWorldStateComponent(
    int maximumQueueLength,
    int tickBudget,
    LiquidPanicPolicy? panicPolicy = null)
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
    PanicPolicy = panicPolicy ?? LiquidPanicPolicy.CreateDefault(maximumQueueLength, tickBudget);
    if (PanicPolicy.HighWaterQueueLength > maximumQueueLength ||
        PanicPolicy.PanicTickBudget > maximumQueueLength)
    {
      throw new ArgumentOutOfRangeException(nameof(panicPolicy));
    }
  }

  public int EffectiveTickBudget => Mode == LiquidSimulationMode.Panic
    ? PanicPolicy.PanicTickBudget
    : TickBudget;
  public int MaximumQueueLength { get; }
  public LiquidSimulationMode Mode { get; private set; }
  public LiquidPanicPolicy PanicPolicy { get; }
  public int TickBudget { get; }

  public void ObserveQueueLength(int queueLength)
  {
    if (queueLength < 0 || queueLength > MaximumQueueLength)
    {
      throw new ArgumentOutOfRangeException(nameof(queueLength));
    }

    if (Mode == LiquidSimulationMode.Panic)
    {
      if (queueLength <= PanicPolicy.RecoveryQueueLength)
      {
        Mode = LiquidSimulationMode.Normal;
        _sustainedHighWaterTicks = 0;
      }

      return;
    }

    if (Mode != LiquidSimulationMode.Normal)
    {
      return;
    }

    if (queueLength < PanicPolicy.HighWaterQueueLength)
    {
      _sustainedHighWaterTicks = 0;
      return;
    }

    if (_sustainedHighWaterTicks < PanicPolicy.SustainedHighWaterTicks)
    {
      _sustainedHighWaterTicks++;
    }

    if (_sustainedHighWaterTicks >= PanicPolicy.SustainedHighWaterTicks)
    {
      Mode = LiquidSimulationMode.Panic;
    }
  }

  public void SetMode(LiquidSimulationMode mode)
  {
    if (!Enum.IsDefined(mode))
    {
      throw new ArgumentOutOfRangeException(nameof(mode));
    }

    Mode = mode;
  }
}
