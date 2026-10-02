using System;

namespace Terraria.WorldGeneration.Housing;

public readonly record struct HousingValidationDefinition
{
  public HousingValidationDefinition(
    int maxRoomTiles,
    int maxRoomSize,
    int minimumRoomTiles,
    int tileTypeCount,
    int worldEdgeMargin = 10)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxRoomTiles);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxRoomSize);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(minimumRoomTiles);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tileTypeCount);
    ArgumentOutOfRangeException.ThrowIfNegative(worldEdgeMargin);

    MaxRoomTiles = maxRoomTiles;
    MaxRoomSize = maxRoomSize;
    MinimumRoomTiles = minimumRoomTiles;
    TileTypeCount = tileTypeCount;
    WorldEdgeMargin = worldEdgeMargin;
  }

  public int MaxRoomTiles { get; }

  public int MaxRoomSize { get; }

  public int MinimumRoomTiles { get; }

  public int TileTypeCount { get; }

  public int WorldEdgeMargin { get; }
}
