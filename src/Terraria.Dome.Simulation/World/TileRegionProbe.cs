using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldModel;

public static class TileRegionProbe
{
  private const byte LavaLiquidType = 1;
  private const byte ShimmerLiquidType = 3;
  private const ushort DungeonWallType = 244;
  private const ushort IceTileType = 147;
  private const ushort IceBrickTileType = 161;
  private const ushort RockTileType = 1;
  private const ushort SandTileType = 53;
  private const ushort SandstoneTileType = 396;
  private const ushort HardenedSandTileType = 397;
  private const ushort ShroomTileType = 70;

  public static TileRegionProbeResult CountOpenTiles(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    TileRegionProbeOptions options)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);

    int tileCount = 0;
    int lavaTileCount = 0;
    int shroomTileCount = 0;
    int iceTileCount = 0;
    int sandTileCount = 0;
    int rockTileCount = 0;
    HashSet<(int X, int Y)> countedTiles = new();
    Stack<(int X, int Y)> pendingTiles = new();
    pendingTiles.Push((x, y));

    while (pendingTiles.Count > 0 && tileCount < options.MaximumTiles)
    {
      (int candidateX, int candidateY) = pendingTiles.Pop();
      if (IsWorldEdge(snapshot, candidateX, candidateY))
      {
        tileCount = options.MaximumTiles;
        break;
      }

      if (countedTiles.Contains((candidateX, candidateY)))
      {
        continue;
      }

      WorldTile tile = snapshot.GetTile(candidateX, candidateY);
      if (tile.WallType == DungeonWallType || HasShimmer(tile) ||
          (!options.Jungle && tile.WallType != 0))
      {
        tileCount = options.MaximumTiles;
        break;
      }

      if (HasLava(tile))
      {
        lavaTileCount++;
        if (!options.LavaOk)
        {
          tileCount = options.MaximumTiles;
          break;
        }
      }

      if (tile.IsActive)
      {
        CountActiveTile(
          tile,
          ref shroomTileCount,
          ref iceTileCount,
          ref sandTileCount,
          ref rockTileCount);
      }

      if (TileStateQuery.IsSolid(tile, tileDefinitions))
      {
        continue;
      }

      countedTiles.Add((candidateX, candidateY));
      tileCount++;
      // Reverse push order preserves legacy recursive traversal: left, right, up, down.
      pendingTiles.Push((candidateX, candidateY + 1));
      pendingTiles.Push((candidateX, candidateY - 1));
      pendingTiles.Push((candidateX + 1, candidateY));
      pendingTiles.Push((candidateX - 1, candidateY));
    }

    return new TileRegionProbeResult(
      tileCount,
      tileCount >= options.MaximumTiles,
      lavaTileCount,
      shroomTileCount,
      iceTileCount,
      sandTileCount,
      rockTileCount);
  }

  private static void CountActiveTile(
    WorldTile tile,
    ref int shroomTileCount,
    ref int iceTileCount,
    ref int sandTileCount,
    ref int rockTileCount)
  {
    switch (tile.Type)
    {
      case ShroomTileType:
        shroomTileCount++;
        break;
      case RockTileType:
        rockTileCount++;
        break;
      case IceTileType:
      case IceBrickTileType:
        iceTileCount++;
        break;
      case SandTileType:
      case SandstoneTileType:
      case HardenedSandTileType:
        sandTileCount++;
        break;
    }
  }

  private static bool HasLava(WorldTile tile)
  {
    return tile.LiquidAmount > 0 && tile.LiquidType == LavaLiquidType;
  }

  private static bool HasShimmer(WorldTile tile)
  {
    return tile.LiquidAmount > 0 && tile.LiquidType == ShimmerLiquidType;
  }

  private static bool IsWorldEdge(WorldGridSnapshot snapshot, int x, int y)
  {
    return x <= 1 || x >= snapshot.Metadata.Width - 1 ||
      y <= 1 || y >= snapshot.Metadata.Height - 1;
  }
}
