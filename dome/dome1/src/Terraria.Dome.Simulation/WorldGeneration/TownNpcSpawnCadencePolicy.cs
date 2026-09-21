using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TownNpcSpawnCadencePolicy
{
  public static int CalculatePeriod(int worldUpdateRate)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(worldUpdateRate);
    return checked(20 * worldUpdateRate);
  }

  public static TownNpcSpawnCadenceDecision Advance(
    int delay,
    int period,
    bool invasionActive,
    bool eclipseActive)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(delay);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(period);
    if (delay > period)
    {
      throw new ArgumentOutOfRangeException(nameof(delay));
    }

    bool blockedByEvent = invasionActive || eclipseActive;
    if (blockedByEvent)
    {
      return new TownNpcSpawnCadenceDecision(delay, period, delay, false, true);
    }

    int nextDelay = checked(delay + 1);
    bool shouldAttemptSpawn = nextDelay >= period;
    if (shouldAttemptSpawn)
    {
      nextDelay = 0;
    }

    return new TownNpcSpawnCadenceDecision(
      delay,
      period,
      nextDelay,
      shouldAttemptSpawn,
      false);
  }
}
