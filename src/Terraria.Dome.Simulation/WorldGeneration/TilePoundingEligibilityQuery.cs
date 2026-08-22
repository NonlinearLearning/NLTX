using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TilePoundingEligibilityQuery
{
  private const ushort DungeonDoorTileType = 10;
  private const ushort DyePlantTileType = 190;
  private const ushort RollingCactusTileType = 484;
  private const ushort SandstoneTileType = 30;
  private const ushort SpecialPlatformTileType = 380;

  public static bool CanPound(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    IReadOnlySet<ushort> boulderTileTypes,
    bool isGeneratingOrLoadingWorld,
    Func<int, int, bool> canKillTile)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(boulderTileTypes);
    ArgumentNullException.ThrowIfNull(canKillTile);
    if (!snapshot.Metadata.IsInside(x, y) || !snapshot.Metadata.IsInside(x, y - 1) ||
        !snapshot.Metadata.IsInside(x, y + 1))
    {
      return false;
    }

    WorldTile tile = snapshot.GetTile(x, y);
    if (IsPoundingBlockedTileType(tile.Type) || boulderTileTypes.Contains(tile.Type))
    {
      return false;
    }

    if (isGeneratingOrLoadingWorld && tile.Type is DyePlantTileType or SandstoneTileType)
    {
      return false;
    }

    WorldTile aboveTile = snapshot.GetTile(x, y - 1);
    if (aboveTile.IsActive && TileSlopingQuery.ForbidsSloping(aboveTile.Type))
    {
      return false;
    }

    return canKillTile(x, y);
  }

  private static bool IsPoundingBlockedTileType(ushort tileType)
  {
    return tileType is DungeonDoorTileType or 48 or 137 or 232 or SpecialPlatformTileType or
      387 or 388 or 476 or RollingCactusTileType;
  }
}
