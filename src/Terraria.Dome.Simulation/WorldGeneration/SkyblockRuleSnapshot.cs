namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct SkyblockRuleSnapshot(
  bool NoAltars,
  bool NoDungeon,
  bool NoTemple,
  bool NoHellstone,
  bool NoFossils,
  bool NoLifeCrystals,
  bool NoHellforge,
  bool LowTiles);
