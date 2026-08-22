using System;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldModel;

public static class TileRopeQuery
{
  private const ushort PlatformBridgeTileType = 314;
  private const ushort SpecialPlatformBridgeTileType = 380;

  public static TileRopeEnds FindEnds(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    bool treatEmptyAsRopeEnd = false,
    int rangeToCheck = 5)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentOutOfRangeException.ThrowIfNegative(rangeToCheck);

    int topY = -1;
    int bottomY = -1;
    for (int distance = 1; distance <= rangeToCheck && snapshot.Metadata.IsInside(x, y - distance);
         distance++)
    {
      int candidateY = y - distance;
      WorldTile tile = snapshot.GetTile(x, candidateY);
      if (!tile.IsActive)
      {
        if (treatEmptyAsRopeEnd)
        {
          topY = candidateY;
        }

        break;
      }

      if (IsRopeTile(tile))
      {
        topY = candidateY;
        break;
      }
    }

    int offset = y - topY;
    for (int distance = 1 + offset;
         distance <= rangeToCheck + 1 && snapshot.Metadata.IsInside(x, topY + distance);
         distance++)
    {
      int candidateY = topY + distance;
      WorldTile tile = snapshot.GetTile(x, candidateY);
      if (!tile.IsActive)
      {
        if (treatEmptyAsRopeEnd)
        {
          bottomY = candidateY;
        }

        break;
      }

      if (IsRopeTile(tile))
      {
        bottomY = candidateY;
        break;
      }
    }

    return new TileRopeEnds(topY, bottomY);
  }

  public static bool IsRope(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    int rangeToCheck = 5)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    ArgumentOutOfRangeException.ThrowIfNegative(rangeToCheck);

    WorldTile tile = snapshot.GetTile(x, y);
    if (!tile.IsActive)
    {
      return false;
    }

    if (IsRopeTile(tile))
    {
      return true;
    }

    if (tile.Type != PlatformBridgeTileType && tile.Type != SpecialPlatformBridgeTileType &&
        (!tileDefinitions.TryGet(tile.Type, out TileDefinition definition) ||
        !definition.IsPlatform))
    {
      return false;
    }

    TileRopeEnds ends = FindEnds(snapshot, x, y, rangeToCheck: rangeToCheck);
    return ends.TopY != -1 && ends.BottomY != -1;
  }

  private static bool IsRopeTile(WorldTile tile)
  {
    return tile.Type is 213 or 214 or 353 or 365 or 366 or 449 or 450 or 451 or 504;
  }
}
