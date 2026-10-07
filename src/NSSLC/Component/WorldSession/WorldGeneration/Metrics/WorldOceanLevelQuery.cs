namespace Terraria.WorldGeneration.Metrics;

public static class WorldOceanLevelQuery
{
  private const double SurfaceOffset = 40.0;

  public static double Evaluate(double worldSurface, double rockLayer)
  {
    return (worldSurface + rockLayer) / 2.0 + SurfaceOffset;
  }
}
