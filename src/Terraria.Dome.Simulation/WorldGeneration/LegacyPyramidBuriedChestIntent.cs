namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyPyramidBuriedChestIntent(
  long Sequence,
  int TileX,
  int TileY,
  int MainItemInChest,
  bool NotNearOtherChests,
  int ChestStyle,
  bool TrySlope,
  ushort ChestTileType,
  string Source,
  int SourceLine);
