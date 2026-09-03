namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyErrorWorldTileDefinition(
  ushort TileType,
  bool IsSolid,
  bool IsSolidTop,
  bool IsFrameImportant,
  bool IsDungeon);
