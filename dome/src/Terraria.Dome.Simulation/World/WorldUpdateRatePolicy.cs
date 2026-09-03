using System;

namespace Terraria.Dome.Simulation.WorldModel;

public static class WorldUpdateRatePolicy
{
  private const int MaximumWorldTilesUpdateRate = 24;

  public static int GetRate(int desiredRate, bool isTimeFrozen)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(desiredRate);
    return isTimeFrozen ? 0 : Math.Min(desiredRate, MaximumWorldTilesUpdateRate);
  }
}
