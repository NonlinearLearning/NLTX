using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileMergeQuery
{
  public static TileMergeFrametestResult ApplyFrametest(
    int tileType,
    int lookForTileType,
    TileMergeNeighbors neighbors,
    bool mergeUp,
    bool mergeDown,
    bool mergeLeft,
    bool mergeRight)
  {
    return ApplyFrametest(
      tileType,
      neighbors,
      candidate => candidate == lookForTileType,
      mergeUp,
      mergeDown,
      mergeLeft,
      mergeRight);
  }

  public static TileMergeFrametestResult ApplyFrametest(
    int tileType,
    IReadOnlySet<int> lookForTileTypes,
    TileMergeNeighbors neighbors,
    bool mergeUp,
    bool mergeDown,
    bool mergeLeft,
    bool mergeRight)
  {
    ArgumentNullException.ThrowIfNull(lookForTileTypes);
    return ApplyFrametest(
      tileType,
      neighbors,
      candidate => candidate >= 0 && lookForTileTypes.Contains(candidate),
      mergeUp,
      mergeDown,
      mergeLeft,
      mergeRight);
  }

  public static TileMergeNeighbors ReplaceCardinal(
    int tileType,
    int lookForTileType,
    TileMergeNeighbors neighbors)
  {
    return neighbors with
    {
      Up = ReplaceIfMatch(neighbors.Up, tileType, candidate => candidate == lookForTileType),
      Down = ReplaceIfMatch(neighbors.Down, tileType, candidate => candidate == lookForTileType),
      Left = ReplaceIfMatch(neighbors.Left, tileType, candidate => candidate == lookForTileType),
      Right = ReplaceIfMatch(neighbors.Right, tileType, candidate => candidate == lookForTileType)
    };
  }

  public static TileMergeNeighbors ReplaceCardinal(
    int tileType,
    IReadOnlySet<int> lookForTileTypes,
    TileMergeNeighbors neighbors)
  {
    ArgumentNullException.ThrowIfNull(lookForTileTypes);
    return Replace(tileType, neighbors, candidate =>
      candidate >= 0 && lookForTileTypes.Contains(candidate), includeDiagonals: false);
  }

  public static TileMergeNeighbors ReplaceAll(
    int tileType,
    int lookForTileType,
    TileMergeNeighbors neighbors)
  {
    return Replace(tileType, neighbors, candidate => candidate == lookForTileType, true);
  }

  public static TileMergeNeighbors ReplaceAll(
    int tileType,
    IReadOnlySet<int> lookForTileTypes,
    TileMergeNeighbors neighbors)
  {
    ArgumentNullException.ThrowIfNull(lookForTileTypes);
    return Replace(tileType, neighbors, candidate =>
      candidate >= 0 && lookForTileTypes.Contains(candidate), includeDiagonals: true);
  }

  public static TileMergeNeighbors ReplaceAllExcept(
    int tileType,
    IReadOnlySet<int> lookForTileTypes,
    int excludedTileType,
    TileMergeNeighbors neighbors)
  {
    ArgumentNullException.ThrowIfNull(lookForTileTypes);
    return Replace(tileType, neighbors, candidate =>
      candidate >= 0 && candidate != excludedTileType && lookForTileTypes.Contains(candidate),
      includeDiagonals: true);
  }

  public static TileMergeNeighbors ReplaceAllExcept(
    int tileType,
    IReadOnlySet<int> lookForTileTypes,
    IReadOnlySet<int> excludedTileTypes,
    TileMergeNeighbors neighbors)
  {
    ArgumentNullException.ThrowIfNull(excludedTileTypes);
    ArgumentNullException.ThrowIfNull(lookForTileTypes);
    return Replace(tileType, neighbors, candidate =>
      candidate >= 0 && !excludedTileTypes.Contains(candidate) &&
      lookForTileTypes.Contains(candidate),
      includeDiagonals: true);
  }

  public static TileMergeNeighbors ReplaceDifferentExcept(
    int tileType,
    int replacementTileType,
    IReadOnlySet<int> excludedTileTypes,
    TileMergeNeighbors neighbors)
  {
    ArgumentNullException.ThrowIfNull(excludedTileTypes);
    return Replace(replacementTileType, neighbors, candidate =>
      candidate >= 0 && candidate != tileType && !excludedTileTypes.Contains(candidate), true);
  }

  private static TileMergeFrametestResult ApplyFrametest(
    int tileType,
    TileMergeNeighbors neighbors,
    Func<int, bool> shouldReplace,
    bool mergeUp,
    bool mergeDown,
    bool mergeLeft,
    bool mergeRight)
  {
    bool frameUp = shouldReplace(neighbors.Up);
    bool frameDown = shouldReplace(neighbors.Down);
    bool frameLeft = shouldReplace(neighbors.Left);
    bool frameRight = shouldReplace(neighbors.Right);
    TileMergeNeighbors updated = neighbors with
    {
      Up = frameUp && mergeDown ? tileType : neighbors.Up,
      Down = frameDown && mergeUp ? tileType : neighbors.Down,
      Left = frameLeft && mergeRight ? tileType : neighbors.Left,
      Right = frameRight && mergeLeft ? tileType : neighbors.Right,
      UpLeft = ReplaceIfMatch(neighbors.UpLeft, tileType, shouldReplace),
      UpRight = ReplaceIfMatch(neighbors.UpRight, tileType, shouldReplace),
      DownLeft = ReplaceIfMatch(neighbors.DownLeft, tileType, shouldReplace),
      DownRight = ReplaceIfMatch(neighbors.DownRight, tileType, shouldReplace)
    };
    return new TileMergeFrametestResult(updated, frameUp, frameDown, frameLeft, frameRight);
  }

  private static TileMergeNeighbors Replace(
    int tileType,
    TileMergeNeighbors neighbors,
    Func<int, bool> shouldReplace,
    bool includeDiagonals)
  {
    return neighbors with
    {
      Up = ReplaceIfMatch(neighbors.Up, tileType, shouldReplace),
      Down = ReplaceIfMatch(neighbors.Down, tileType, shouldReplace),
      Left = ReplaceIfMatch(neighbors.Left, tileType, shouldReplace),
      Right = ReplaceIfMatch(neighbors.Right, tileType, shouldReplace),
      UpLeft = includeDiagonals
        ? ReplaceIfMatch(neighbors.UpLeft, tileType, shouldReplace)
        : neighbors.UpLeft,
      UpRight = includeDiagonals
        ? ReplaceIfMatch(neighbors.UpRight, tileType, shouldReplace)
        : neighbors.UpRight,
      DownLeft = includeDiagonals
        ? ReplaceIfMatch(neighbors.DownLeft, tileType, shouldReplace)
        : neighbors.DownLeft,
      DownRight = includeDiagonals
        ? ReplaceIfMatch(neighbors.DownRight, tileType, shouldReplace)
        : neighbors.DownRight
    };
  }

  private static int ReplaceIfMatch(int candidate, int tileType, Func<int, bool> shouldReplace)
  {
    return shouldReplace(candidate) ? tileType : candidate;
  }
}
