namespace Terraria.Dome.Simulation.WorldModel.Definitions;

public readonly record struct TileDefinition(
  ushort TileType,
  bool BlocksLiquid,
  bool IsPlatform,
  bool IsNoAttach,
  bool WaterDestroysTile,
  bool LavaDestroysTile);
