namespace Terraria.WorldGeneration.Components;

public sealed class WorldDescriptorState
{
  public int WorldId;
  public Guid UniqueId;
  public string Name = string.Empty;
  public string SeedText = string.Empty;
  public ulong GeneratorVersion;

  public int SizeX;
  public int SizeY;
  public WorldBounds Bounds;

  public double SurfaceLayer;
  public double RockLayer;
  public int SpawnTileX;
  public int SpawnTileY;
  public int DungeonTileX;
  public int DungeonTileY;

  public int SectionCountX => SizeX / 200;
  public int SectionCountY => SizeY / 150;
  public bool HasSurface => SurfaceLayer > 50.0;
}
