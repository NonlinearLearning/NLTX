using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyWallFrameNeighborQuery
{
  public static bool TryEvaluate(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    bool showInvisibleWalls,
    out LegacyWallFrameNeighborResult result,
    out string? failureReason)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    result = default;
    failureReason = null;
    if (x <= 0 || y <= 0 || x >= snapshot.Metadata.Width - 1 ||
        y >= snapshot.Metadata.Height - 1)
    {
      failureReason = "WallFrame coordinates must be strictly inside the world.";
      return false;
    }

    WorldTile center = snapshot.GetTile(x, y);
    ushort originalWallType = center.WallType;
    bool wasNormalized = originalWallType >= LegacyLargeFrameWallRegistry.WallTypeCount;
    ushort effectiveWallType = wasNormalized ? (ushort)0 : originalWallType;
    if (effectiveWallType == 0)
    {
      result = new LegacyWallFrameNeighborResult(
        x,
        y,
        originalWallType,
        effectiveWallType,
        LegacyWallFrameNeighborMask.None,
        showInvisibleWalls,
        ShouldFrame: false,
        wasNormalized);
      return true;
    }

    LegacyWallFrameNeighborMask mask = LegacyWallFrameNeighborMask.None;
    if (ContributesToMask(snapshot.GetTile(x, y - 1), showInvisibleWalls))
    {
      mask |= LegacyWallFrameNeighborMask.Above;
    }

    if (ContributesToMask(snapshot.GetTile(x - 1, y), showInvisibleWalls))
    {
      mask |= LegacyWallFrameNeighborMask.Left;
    }

    if (ContributesToMask(snapshot.GetTile(x + 1, y), showInvisibleWalls))
    {
      mask |= LegacyWallFrameNeighborMask.Right;
    }

    if (ContributesToMask(snapshot.GetTile(x, y + 1), showInvisibleWalls))
    {
      mask |= LegacyWallFrameNeighborMask.Below;
    }

    result = new LegacyWallFrameNeighborResult(
      x,
      y,
      originalWallType,
      effectiveWallType,
      mask,
      showInvisibleWalls,
      ShouldFrame: true,
      wasNormalized);
    return true;
  }

  private static bool ContributesToMask(WorldTile tile, bool showInvisibleWalls)
  {
    bool hasWallOrTruncatingTile = tile.WallType > 0 ||
      (tile.IsActive && LegacyTruncatingWallTileRegistry.IsTruncatingTile(tile.Type));
    return hasWallOrTruncatingTile && (showInvisibleWalls || !tile.IsInvisibleWall);
  }
}
