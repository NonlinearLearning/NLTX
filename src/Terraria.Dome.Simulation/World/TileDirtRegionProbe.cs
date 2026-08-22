using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldModel;

public static class TileDirtRegionProbe
{
  private const ushort CorruptGrassWallType = 3;
  private const ushort DirtWallType = 2;
  private const ushort DungeonWallType = 244;
  private const ushort IceBrickTileType = 161;
  private const ushort IceTileType = 147;
  private const ushort JungleWallType = 59;
  private const ushort LihzahrdBrickWallType = 216;
  private const ushort MarbleWallType = 187;
  private const ushort SpiderWallType = 83;

  public static TileDirtRegionProbeResult CountTiles(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    TileDirtRegionProbeOptions options)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);

    int tileCount = 0;
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
      if (IsIceTile(tile) || IsBlockedWall(tile.WallType))
      {
        tileCount = options.MaximumTiles;
        break;
      }

      if (TileStateQuery.IsSolid(tile, tileDefinitions) || !IsDirtRegionWall(tile.WallType))
      {
        continue;
      }

      countedTiles.Add((candidateX, candidateY));
      tileCount++;
      PushLegacyNeighbors(pendingTiles, candidateX, candidateY);
    }

    return new TileDirtRegionProbeResult(tileCount, tileCount >= options.MaximumTiles);
  }

  private static bool IsBlockedWall(ushort wallType)
  {
    return wallType is DungeonWallType or SpiderWallType or CorruptGrassWallType or
      MarbleWallType or LihzahrdBrickWallType;
  }

  private static bool IsDirtRegionWall(ushort wallType)
  {
    return wallType is DirtWallType or JungleWallType;
  }

  private static bool IsIceTile(WorldTile tile)
  {
    return tile.IsActive && tile.Type is IceTileType or IceBrickTileType;
  }

  private static bool IsWorldEdge(WorldGridSnapshot snapshot, int x, int y)
  {
    return x <= 1 || x >= snapshot.Metadata.Width - 1 ||
      y <= 1 || y >= snapshot.Metadata.Height - 1;
  }

  private static void PushLegacyNeighbors(Stack<(int X, int Y)> pendingTiles, int x, int y)
  {
    // Stack order preserves recursive visitation: left, right, up, down, diagonals, then +/- two X.
    pendingTiles.Push((x + 2, y));
    pendingTiles.Push((x - 2, y));
    pendingTiles.Push((x + 1, y + 1));
    pendingTiles.Push((x + 1, y - 1));
    pendingTiles.Push((x - 1, y + 1));
    pendingTiles.Push((x - 1, y - 1));
    pendingTiles.Push((x, y + 1));
    pendingTiles.Push((x, y - 1));
    pendingTiles.Push((x + 1, y));
    pendingTiles.Push((x - 1, y));
  }
}
