namespace Terraria.WorldGeneration.Dungeon.Styles;

public sealed record DungeonStyleMaterialDefinition(
  DungeonStyleId Style,
  int BrickTileType,
  int BrickGrassTileType,
  int BrickCrackedTileType,
  int BrickWallType,
  int WindowGlassWallType,
  int WindowClosedGlassWallType,
  int WindowEdgeWallType,
  int PitTrapTileType,
  int LiquidType,
  int UnbreakableWallProgressionTier,
  bool EdgeDither);
