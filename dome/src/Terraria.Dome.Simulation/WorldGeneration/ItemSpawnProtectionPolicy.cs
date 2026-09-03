using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class ItemSpawnProtectionPolicy
{
  public const int Version4DurationTicks = 18000;

  public static int ValidateDuration(int durationTicks)
  {
    if (durationTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(durationTicks));
    }

    return durationTicks;
  }
}
