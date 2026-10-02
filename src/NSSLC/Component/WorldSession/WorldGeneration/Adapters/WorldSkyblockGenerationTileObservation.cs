namespace Terraria.WorldGeneration.Adapters;

public readonly record struct WorldSkyblockGenerationTileObservation(
  bool IsActive,
  ushort TileType,
  ushort WallType);
