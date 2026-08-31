using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TilePoundingEligibilityQuery
{
  private static readonly IReadOnlySet<ushort> BlockedTileTypes = new HashSet<ushort>
  {
    10, 48, 137, 232, 380, 387, 388, 476, 484
  }.ToFrozenSet();
  private static readonly IReadOnlySet<ushort> GenerationBlockedTileTypes = new HashSet<ushort>
  {
    190, 30
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterBlockedDefaults()
  {
    return BlockedTileTypes;
  }

  public static IReadOnlySet<ushort> RegisterGenerationBlockedDefaults()
  {
    return GenerationBlockedTileTypes;
  }

  public static bool CanPound(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    bool isGeneratingOrLoadingWorld,
    Func<int, int, bool> canKillTile)
  {
    return CanPound(
      snapshot,
      x,
      y,
      BoulderTileRegistry.RegisterDefaults(),
      isGeneratingOrLoadingWorld,
      canKillTile);
  }

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

    if (isGeneratingOrLoadingWorld && GenerationBlockedTileTypes.Contains(tile.Type))
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
    return BlockedTileTypes.Contains(tileType);
  }
}
