using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyNoSurfacePolicy
{
  private const int WorldWidthPerMeteorAttempt = 1050;

  public static LegacyNoSurfaceActionPlan Create(
    bool skyblockWorld,
    bool remixWorld,
    bool worldSpawnHasBeenRandomized,
    int worldWidth)
  {
    if (worldWidth < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldWidth));
    }

    if (skyblockWorld)
    {
      return default;
    }

    bool randomizeSpawn = !remixWorld && !worldSpawnHasBeenRandomized;
    return new LegacyNoSurfaceActionPlan(
      worldWidth / WorldWidthPerMeteorAttempt,
      randomizeSpawn,
      randomizeSpawn);
  }
}
