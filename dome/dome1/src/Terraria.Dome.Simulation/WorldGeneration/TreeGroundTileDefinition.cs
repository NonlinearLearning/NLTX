namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct TreeGroundTileDefinition(
  ushort TileType,
  bool IsStone,
  bool IsMoss,
  bool IsGrass);
