namespace Terraria.Dome.Simulation.WorldGeneration;

public static class DunesAndPyramidGenerationGate
{
  public static bool ShouldRunDunes(
    bool isSkyblockWorld,
    bool isNoSurfaceWorld)
  {
    return !isSkyblockWorld && !isNoSurfaceWorld;
  }

  public static bool ShouldRunPyramids(
    bool isSkyblockWorld,
    bool noSurfaceNoPyramids)
  {
    return !isSkyblockWorld && !noSurfaceNoPyramids;
  }

  public static bool ShouldRun(
    bool isSkyblockWorld,
    bool isNoSurfaceWorld,
    bool noSurfaceNoPyramids)
  {
    return ShouldRunDunes(isSkyblockWorld, isNoSurfaceWorld) &&
      ShouldRunPyramids(isSkyblockWorld, noSurfaceNoPyramids);
  }
}
