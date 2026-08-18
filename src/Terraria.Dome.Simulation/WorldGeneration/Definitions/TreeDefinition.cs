using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly struct TreeDefinition
{
  public TreeDefinition(
    string id,
    ushort trunkTileType,
    ushort leafTileType,
    int minimumHeight,
    int maximumHeight,
    int canopyRadius)
    : this()
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(id);
    if (minimumHeight <= 0 || maximumHeight < minimumHeight || canopyRadius < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(MinimumHeight));
    }

    Id = id;
    TrunkTileType = trunkTileType;
    LeafTileType = leafTileType;
    MinimumHeight = minimumHeight;
    MaximumHeight = maximumHeight;
    CanopyRadius = canopyRadius;
  }

  public string Id { get; }

  public ushort TrunkTileType { get; }

  public ushort LeafTileType { get; }

  public int MinimumHeight { get; }

  public int MaximumHeight { get; }

  public int CanopyRadius { get; }
}
