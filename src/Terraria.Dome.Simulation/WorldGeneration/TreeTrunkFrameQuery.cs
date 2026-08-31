using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TreeTrunkFrameQuery
{
  private const int WorldMargin = 2;
  private const ushort PalmTreeTileType = 323;

  public static bool TryGetBranchOffset(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    out int offsetToTrunk)
  {
    return TryGetBranchOffset(
      snapshot,
      x,
      y,
      TreeTrunkTileRegistry.RegisterDefaults(),
      out offsetToTrunk);
  }

  public static bool TryGetRootOffset(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    out int offsetToTrunk)
  {
    return TryGetRootOffset(
      snapshot,
      x,
      y,
      TreeTrunkTileRegistry.RegisterDefaults(),
      out offsetToTrunk);
  }

  public static bool TryGetBranchOffset(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    IReadOnlySet<ushort> treeTrunkTypes,
    out int offsetToTrunk)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(treeTrunkTypes);
    offsetToTrunk = 0;
    if (!IsTreeTrunk(snapshot, x, y, treeTrunkTypes, out WorldTile tile))
    {
      return false;
    }

    if ((tile.FrameX == 44 && tile.FrameY is 198 or 220 or 242) ||
        (tile.FrameX == 66 && tile.FrameY is 0 or 22 or 44))
    {
      offsetToTrunk = 1;
      return true;
    }

    if ((tile.FrameX == 66 && tile.FrameY is 198 or 220 or 242) ||
        (tile.FrameX == 88 && tile.FrameY is 66 or 88 or 110))
    {
      offsetToTrunk = -1;
      return true;
    }

    return false;
  }

  public static bool TryGetRootOffset(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    IReadOnlySet<ushort> treeTrunkTypes,
    out int offsetToTrunk)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(treeTrunkTypes);
    offsetToTrunk = 0;
    if (!IsTreeTrunk(snapshot, x, y, treeTrunkTypes, out WorldTile tile))
    {
      return false;
    }

    if (tile.FrameX == 44 && tile.FrameY is 132 or 154 or 176)
    {
      offsetToTrunk = 1;
      return true;
    }

    if (tile.FrameX == 22 && tile.FrameY is 132 or 154 or 176)
    {
      offsetToTrunk = -1;
      return true;
    }

    return false;
  }

  private static bool IsTreeTrunk(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    IReadOnlySet<ushort> treeTrunkTypes,
    out WorldTile tile)
  {
    tile = default;
    if (x < WorldMargin || x >= snapshot.Metadata.Width - WorldMargin ||
        y < WorldMargin || y >= snapshot.Metadata.Height - WorldMargin)
    {
      return false;
    }

    tile = snapshot.GetTile(x, y);
    return tile.IsActive && tile.Type != PalmTreeTileType && treeTrunkTypes.Contains(tile.Type);
  }
}
