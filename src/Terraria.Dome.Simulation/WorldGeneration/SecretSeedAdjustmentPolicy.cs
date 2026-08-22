using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class SecretSeedAdjustmentPolicy
{
  public static int Adjust(double value, int activeSecretSeedCount)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(activeSecretSeedCount);
    if (activeSecretSeedCount < 1)
    {
      return 4;
    }

    return (int)(value * ((activeSecretSeedCount + 3) / 4));
  }
}
