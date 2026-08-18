namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct TerrainDefinition(
  ushort GroundTileType,
  int SurfaceVariation,
  int SpawnClearHalfWidth,
  int SpawnClearHeight)
{
  public static TerrainDefinition Default { get; } = new(
    GroundTileType: 1,
    SurfaceVariation: 8,
    SpawnClearHalfWidth: 4,
    SpawnClearHeight: 7);
}
