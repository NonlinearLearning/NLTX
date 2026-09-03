using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct BiomeSurfaceDefinition
{
  public BiomeSurfaceDefinition(
    string id,
    ushort surfaceTileType,
    ushort surfaceWallType,
    int depth)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(id);
    if (depth <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(depth));
    }

    Id = id;
    SurfaceTileType = surfaceTileType;
    SurfaceWallType = surfaceWallType;
    Depth = depth;
  }

  public string Id { get; }

  public ushort SurfaceTileType { get; }

  public ushort SurfaceWallType { get; }

  public int Depth { get; }
}
