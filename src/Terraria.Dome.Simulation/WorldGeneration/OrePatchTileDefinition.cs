namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct OrePatchTileDefinition(
  ushort TileType,
  bool IsSolid,
  bool IsGrass,
  bool IsDungeon,
  bool IsCloud,
  bool IsSand);
