namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyDungeonPostPassPolicy
{
  public static bool ShouldRunLavaLayerCaverer(
    bool dontStarveWorldGen,
    bool tenthAnniversaryWorldGen,
    bool remixWorldGen)
  {
    return dontStarveWorldGen && !tenthAnniversaryWorldGen && !remixWorldGen;
  }
}
