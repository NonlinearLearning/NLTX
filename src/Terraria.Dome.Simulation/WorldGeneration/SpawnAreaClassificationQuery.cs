namespace Terraria.Dome.Simulation.WorldGeneration;

public static class SpawnAreaClassificationQuery
{
  public static bool IsConsidered(
    int y,
    bool isRemixWorld,
    int remixSurfaceLayerLow,
    int remixSurfaceLayerHigh,
    bool worldSpawnHasBeenRandomized,
    bool hasWorldSurface,
    double worldSurface,
    int underworldLayer)
  {
    if (isRemixWorld)
    {
      return y > remixSurfaceLayerLow && y < remixSurfaceLayerHigh;
    }

    if (worldSpawnHasBeenRandomized || !hasWorldSurface)
    {
      return y > worldSurface && y < underworldLayer;
    }

    return y < worldSurface;
  }
}
