namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyPyramidBuriedChestCommandProjectionResult(
  int OriginX,
  int OriginY,
  ushort ChestTileType,
  int ChestStyle,
  int MainItemInChest,
  int TileCommandCount);
