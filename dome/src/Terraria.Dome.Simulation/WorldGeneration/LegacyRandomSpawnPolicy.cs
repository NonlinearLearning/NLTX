namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyRandomSpawnPolicy
{
  public static LegacyNoSurfaceActionPlan Create(bool worldSpawnHasBeenRandomized)
  {
    if (worldSpawnHasBeenRandomized)
    {
      return default;
    }

    return new LegacyNoSurfaceActionPlan(
      UndergroundMeteorAttempts: 0,
      RandomizeSpawn: true,
      PlaceTorchesAroundSpawn: true);
  }
}
