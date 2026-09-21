using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly struct LegacyTreeProfileDefinition
{
  public LegacyTreeProfileDefinition(
    LegacyTreeProfileKind kind,
    ushort treeTileType,
    ushort saplingTileType,
    int minimumHeight,
    int maximumHeight,
    int topPaddingNeeded)
  {
    if (minimumHeight <= 0 || maximumHeight < minimumHeight || topPaddingNeeded < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(minimumHeight));
    }

    Kind = kind;
    TreeTileType = treeTileType;
    SaplingTileType = saplingTileType;
    MinimumHeight = minimumHeight;
    MaximumHeight = maximumHeight;
    TopPaddingNeeded = topPaddingNeeded;
  }

  public LegacyTreeProfileKind Kind { get; }

  public ushort TreeTileType { get; }

  public ushort SaplingTileType { get; }

  public int MinimumHeight { get; }

  public int MaximumHeight { get; }

  public int TopPaddingNeeded { get; }
}
