using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyPooEverywherePolicy
{
  public static int GetAttemptCount(int width, int height, int activeSecretSeedCount)
  {
    if (width < 0 || height < 0 || activeSecretSeedCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    int divisor = Math.Max(1, (2 + activeSecretSeedCount) / 3);
    return (int)(width * (long)height * 0.0002) / divisor;
  }
}
