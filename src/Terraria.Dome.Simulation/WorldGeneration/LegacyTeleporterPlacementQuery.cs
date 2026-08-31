using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyTeleporterPlacementQuery
{
  private const ushort TeleporterTileType = 235;

  public static bool CanPlace(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    IReadOnlySet<ushort> dungeonWallTypes,
    IReadOnlySet<ushort> cloudTileTypes,
    int x,
    int y,
    int spawnX,
    int spawnY,
    bool moreForcefulPlacement)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    ArgumentNullException.ThrowIfNull(dungeonWallTypes);
    ArgumentNullException.ThrowIfNull(cloudTileTypes);
    if (!snapshot.Metadata.IsInside(x - 1, y - 4) || !snapshot.Metadata.IsInside(x + 1, y + 1))
    {
      return false;
    }

    if (!moreForcefulPlacement &&
        (!IsSolid(snapshot.GetTile(x - 1, y), tileDefinitions) ||
         !IsSolid(snapshot.GetTile(x, y), tileDefinitions) ||
         !IsSolid(snapshot.GetTile(x + 1, y), tileDefinitions)))
    {
      return false;
    }

    int placementY = y - 1;
    WorldTile placement = snapshot.GetTile(x, placementY);
    if (dungeonWallTypes.Contains(placement.WallType) || placement.WallType is 112 or 86 ||
        cloudTileTypes.Contains(snapshot.GetTile(x, placementY + 1).Type))
    {
      return false;
    }

    for (int candidateX = x - 1; candidateX <= x + 1; candidateX++)
    {
      for (int candidateY = placementY - 3; candidateY <= placementY; candidateY++)
      {
        WorldTile candidate = snapshot.GetTile(candidateX, candidateY);
        if (candidate.IsActive || candidate.LiquidAmount > 0)
        {
          return false;
        }
      }
    }

    if (Math.Abs(x - spawnX) + Math.Abs(placementY - spawnY) < 20)
    {
      return false;
    }

    int teleporterDistance = moreForcefulPlacement ? 150 : 300;
    return !IsTileNearby(snapshot, x, placementY, TeleporterTileType, teleporterDistance);
  }

  private static bool IsSolid(WorldTile tile, TileDefinitionRegistry tileDefinitions)
  {
    return tile.IsActive && tileDefinitions.TryGet(tile.Type, out TileDefinition definition) &&
      definition.BlocksLiquid && !definition.IsPlatform;
  }

  private static bool IsTileNearby(
    WorldGridSnapshot snapshot,
    int centerX,
    int centerY,
    ushort tileType,
    int radius)
  {
    int startX = Math.Max(0, centerX - radius);
    int endX = Math.Min(snapshot.Metadata.Width - 1, centerX + radius);
    int startY = Math.Max(0, centerY - radius);
    int endY = Math.Min(snapshot.Metadata.Height - 1, centerY + radius);
    for (int x = startX; x <= endX; x++)
    {
      for (int y = startY; y <= endY; y++)
      {
        if (snapshot.GetTile(x, y).Type == tileType)
        {
          return true;
        }
      }
    }

    return false;
  }
}
