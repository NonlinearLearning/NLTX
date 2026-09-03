using System;

namespace Terraria.Dome.Simulation.Liquid.Components;

public sealed record LiquidPanicPolicy
{
  public LiquidPanicPolicy(
    int highWaterQueueLength,
    int sustainedHighWaterTicks,
    int panicTickBudget,
    int recoveryQueueLength)
  {
    if (highWaterQueueLength <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(highWaterQueueLength));
    }

    if (sustainedHighWaterTicks <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(sustainedHighWaterTicks));
    }

    if (panicTickBudget <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(panicTickBudget));
    }

    if (recoveryQueueLength < 0 || recoveryQueueLength >= highWaterQueueLength)
    {
      throw new ArgumentOutOfRangeException(nameof(recoveryQueueLength));
    }

    HighWaterQueueLength = highWaterQueueLength;
    SustainedHighWaterTicks = sustainedHighWaterTicks;
    PanicTickBudget = panicTickBudget;
    RecoveryQueueLength = recoveryQueueLength;
  }

  public int HighWaterQueueLength { get; }
  public int PanicTickBudget { get; }
  public int RecoveryQueueLength { get; }
  public int SustainedHighWaterTicks { get; }

  public static LiquidPanicPolicy CreateDefault(int maximumQueueLength, int tickBudget)
  {
    if (maximumQueueLength <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumQueueLength));
    }

    if (tickBudget <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tickBudget));
    }

    const int SustainedHighWaterTicks = 3601;
    int panicTickBudget = (int)Math.Min(
      maximumQueueLength,
      (long)tickBudget * 5);
    return new LiquidPanicPolicy(
      maximumQueueLength,
      SustainedHighWaterTicks,
      panicTickBudget,
      maximumQueueLength / 2);
  }
}
