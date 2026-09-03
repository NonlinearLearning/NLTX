using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileCosmeticNeighborQuery
{
  public static TileCosmeticNeighborResult Evaluate(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    IReadOnlySet<ushort> stoneTileTypes)
  {
    return Evaluate(snapshot, x, y, Array.Empty<TileChangeCommand>(), stoneTileTypes);
  }

  public static TileCosmeticNeighborResult Evaluate(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    IReadOnlyCollection<TileChangeCommand> pendingMutations,
    IReadOnlySet<ushort> stoneTileTypes)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(pendingMutations);
    ArgumentNullException.ThrowIfNull(stoneTileTypes);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    WorldTile tile = GetPendingTile(snapshot, x, y, pendingMutations);
    return new TileCosmeticNeighborResult(
      CanonicalizeSource(tile.Type, stoneTileTypes),
      GetCardinal(
        snapshot,
        x,
        y - 1,
        tile,
        pendingMutations,
        stoneTileTypes,
        CardinalDirection.Up),
      GetCardinal(
        snapshot,
        x,
        y + 1,
        tile,
        pendingMutations,
        stoneTileTypes,
        CardinalDirection.Down),
      GetCardinal(
        snapshot,
        x - 1,
        y,
        tile,
        pendingMutations,
        stoneTileTypes,
        CardinalDirection.Left),
      GetCardinal(
        snapshot,
        x + 1,
        y,
        tile,
        pendingMutations,
        stoneTileTypes,
        CardinalDirection.Right),
      GetDiagonal(snapshot, x - 1, y - 1, pendingMutations, stoneTileTypes),
      GetDiagonal(snapshot, x + 1, y - 1, pendingMutations, stoneTileTypes),
      GetDiagonal(snapshot, x - 1, y + 1, pendingMutations, stoneTileTypes),
      GetDiagonal(snapshot, x + 1, y + 1, pendingMutations, stoneTileTypes));
  }

  private static int GetCardinal(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    WorldTile sourceTile,
    IReadOnlyCollection<TileChangeCommand> pendingMutations,
    IReadOnlySet<ushort> stoneTileTypes,
    CardinalDirection direction)
  {
    if (!snapshot.Metadata.IsInside(x, y))
    {
      return -1;
    }

    WorldTile tile = GetPendingTile(snapshot, x, y, pendingMutations);
    if (!tile.IsActive || BlocksDirection(tile.Slope, direction))
    {
      return -1;
    }

    if (sourceTile.Slope != 0 && BlocksSourceDirection(sourceTile.Slope, direction))
    {
      return -1;
    }

    return Canonicalize(tile.Type, stoneTileTypes);
  }

  private static int GetDiagonal(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    IReadOnlyCollection<TileChangeCommand> pendingMutations,
    IReadOnlySet<ushort> stoneTileTypes)
  {
    if (!snapshot.Metadata.IsInside(x, y))
    {
      return -1;
    }

    WorldTile tile = GetPendingTile(snapshot, x, y, pendingMutations);
    return tile.IsActive ? Canonicalize(tile.Type, stoneTileTypes) : -1;
  }

  private static int Canonicalize(ushort tileType, IReadOnlySet<ushort> stoneTileTypes)
  {
    return stoneTileTypes.Contains(tileType) ? 1 : tileType;
  }

  private static int CanonicalizeSource(
    ushort tileType,
    IReadOnlySet<ushort> stoneTileTypes)
  {
    if (stoneTileTypes.Contains(tileType))
    {
      return 1;
    }

    return tileType switch
    {
      668 => 0,
      697 => 51,
      _ => tileType
    };
  }

  private static bool BlocksDirection(byte slope, CardinalDirection direction)
  {
    return direction switch
    {
      CardinalDirection.Up => slope is 3 or 4,
      CardinalDirection.Down => slope is 1 or 2,
      CardinalDirection.Left => slope is 1 or 3,
      CardinalDirection.Right => slope is 2 or 4,
      _ => true
    };
  }

  private static bool BlocksSourceDirection(byte slope, CardinalDirection direction)
  {
    return direction switch
    {
      CardinalDirection.Up => slope is 1 or 2,
      CardinalDirection.Down => slope is 3 or 4,
      CardinalDirection.Left => slope is 2 or 4,
      CardinalDirection.Right => slope is 1 or 3,
      _ => true
    };
  }

  private static WorldTile GetPendingTile(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    IReadOnlyCollection<TileChangeCommand> pendingMutations)
  {
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    return TileMutationProjection.ApplyPending(
      snapshot.GetTile(x, y),
      x,
      y,
      pendingMutations);
  }

  private enum CardinalDirection
  {
    Up,
    Down,
    Left,
    Right
  }
}
