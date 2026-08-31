using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly struct StructureDefinition
{
  public StructureDefinition(
    string id,
    int width,
    int height,
    ushort tileType,
    ushort wallType,
    bool allowReplaceExisting,
    int priority = 0)
    : this()
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(id);
    if (width <= 0 || height <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(Width));
    }

    Id = id;
    Width = width;
    Height = height;
    TileType = tileType;
    WallType = wallType;
    AllowReplaceExisting = allowReplaceExisting;
    Priority = priority;
  }

  public string Id { get; }

  public int Width { get; }

  public int Height { get; }

  public ushort TileType { get; }

  public ushort WallType { get; }

  public bool AllowReplaceExisting { get; }

  public int Priority { get; }
}
