namespace Terraria.WorldGeneration.Definitions;

public readonly record struct WorldSkyblockGenerationRulesSelection(
  bool NoAltars,
  bool NoDungeon,
  bool NoTemple,
  bool NoHellstone,
  bool NoFossils,
  bool NoLifeCrystals,
  bool NoHellforge,
  bool LowTiles,
  long GenerationId,
  ulong ScanVersion);
