using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly struct OreDefinition
{
  public OreDefinition(
    string id,
    ushort tileType,
    int minDepth,
    int maxDepth,
    int veinRadius,
    int priority)
    : this()
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(id);
    if (maxDepth < minDepth || minDepth < 0 || veinRadius < 0 || priority < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(MinDepth));
    }

    Id = id;
    TileType = tileType;
    MinDepth = minDepth;
    MaxDepth = maxDepth;
    VeinRadius = veinRadius;
    Priority = priority;
  }

  public string Id { get; }

  public ushort TileType { get; }

  public int MinDepth { get; }

  public int MaxDepth { get; }

  public int VeinRadius { get; }

  public int Priority { get; }
}
