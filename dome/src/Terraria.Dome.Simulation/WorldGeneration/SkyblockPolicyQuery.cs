using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class SkyblockPolicyQuery
{
  public static SkyblockPolicySnapshot Evaluate(
    bool isSkyblockWorld,
    IReadOnlySet<string> enabledVariants,
    bool tenthAnniversaryWorld = false,
    bool getGoodWorld = false)
  {
    ArgumentNullException.ThrowIfNull(enabledVariants);
    if (!isSkyblockWorld)
    {
      return default;
    }

    bool spawnSolidifier = tenthAnniversaryWorld || !getGoodWorld;
    bool extraFloatingIslands = enabledVariants.Contains("extra-floating-islands");
    bool allowsSomeGeneration = enabledVariants.Contains("world-is-frozen") ||
      enabledVariants.Contains("surface-is-desert") ||
      enabledVariants.Contains("surface-is-mushrooms") ||
      enabledVariants.Contains("world-is-infected") ||
      enabledVariants.Contains("hallow-on-the-surface") ||
      enabledVariants.Contains("no-infection") ||
      extraFloatingIslands || enabledVariants.Contains("extra-liquid") ||
      enabledVariants.Contains("extra-living-trees");

    return new SkyblockPolicySnapshot(
      DenyFloatingIslands: !extraFloatingIslands,
      DenyAllGeneration: true,
      DenySomeGeneration: !allowsSomeGeneration,
      SpawnSolidifier: spawnSolidifier,
      SpawnShimmerPool: spawnSolidifier);
  }
}
