using System;

namespace Terraria.WorldGeneration.Components;

public sealed class WorldTerrainStateComponent
{
  public WorldTerrainStateComponent(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
  }

  public long GenerationId { get; }

  public ulong TerrainRevision { get; init; }

  public int? UnderworldLayerY { get; init; }

  public int? OceanLevelY { get; init; }

  public int? BeachDistance { get; init; }

  public int? BeachSandDepth { get; init; }

  public double? SurfaceOffset { get; init; }

  public TilePosition? Jungle { get; init; }

  public TilePosition? Snow { get; init; }

  public TilePosition? Desert { get; init; }

  public bool? SurfaceIsDesert { get; init; }

  public bool? SurfaceIsMushrooms { get; init; }

  public bool? SurfaceIsInSpace { get; init; }

  public bool? IsOceanAtSpawn { get; init; }

  public bool? IsBeachAtSpawn { get; init; }

  public bool? IsNoSurface { get; init; }

  public bool? IsRemix { get; init; }

  public bool? IsErrorWorld { get; init; }
}
