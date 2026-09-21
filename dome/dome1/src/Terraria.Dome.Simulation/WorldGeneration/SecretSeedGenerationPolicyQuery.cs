using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class SecretSeedGenerationPolicyQuery
{
  public static bool ShouldGenerateBiggerAbandonedHouses(
    IReadOnlySet<string> enabledVariants,
    LegacyPassRandomState random)
  {
    ArgumentNullException.ThrowIfNull(enabledVariants);
    ArgumentNullException.ThrowIfNull(random);
    if (enabledVariants.Contains("bigger-abandoned-houses"))
    {
      return true;
    }

    return enabledVariants.Contains("error-world") && random.Next(3) == 0;
  }

  public static bool ShouldGenerateRainbowGlowsticks(
    IReadOnlySet<string> enabledVariants,
    bool isTenthAnniversaryWorld)
  {
    ArgumentNullException.ThrowIfNull(enabledVariants);
    return enabledVariants.Contains("rainbow-stuff") || isTenthAnniversaryWorld;
  }
}
