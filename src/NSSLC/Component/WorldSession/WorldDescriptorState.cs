using System;

namespace Terraria.WorldSession.Components;

public sealed class WorldDescriptorState
{
  private const int SectionHeight = 150;
  private const int SectionWidth = 200;

  public int WorldId;
  public Guid UniqueId;
  public string Name = string.Empty;
  public string SeedText = string.Empty;
  public ulong WorldGeneratorVersion;
  public int SizeX;
  public int SizeY;
  public double LeftWorld;
  public double RightWorld;
  public double TopWorld;
  public double BottomWorld;
  public double SurfaceLayer;
  public double RockLayer;
  public int SpawnTileX;
  public int SpawnTileY;
  public int DungeonTileX;
  public int DungeonTileY;

  public int SectionCountX => SizeX / SectionWidth;
  public int SectionCountY => SizeY / SectionHeight;
  public bool HasSurface => SurfaceLayer > 50.0;
  public WorldBounds Bounds => new(
    LeftWorld,
    TopWorld,
    RightWorld,
    BottomWorld);
}
