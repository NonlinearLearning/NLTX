namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyTerrainPassContract(
  string PassName,
  double Weight,
  string ConfigurationSection,
  int FlatBeachPadding,
  bool ResetsRandomFromWorldSeed);

public static class LegacyTerrainPassContractDefinition
{
  public static LegacyTerrainPassContract CreateDefault()
  {
    return new LegacyTerrainPassContract(
      "Terrain",
      449.3721923828125,
      "Terrain",
      5,
      ResetsRandomFromWorldSeed: true);
  }
}
