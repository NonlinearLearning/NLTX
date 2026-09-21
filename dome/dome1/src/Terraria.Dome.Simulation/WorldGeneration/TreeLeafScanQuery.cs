using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TreeLeafScanQuery
{
  public static TreeLeafScanResult Scan(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    int hollowTreeFoliageStyle)
  {
    return Scan(
      snapshot,
      x,
      y,
      TreeLeafCheckedTypeRegistry.RegisterDefaults(),
      hollowTreeFoliageStyle);
  }

  public static TreeLeafScanResult Scan(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    IReadOnlySet<ushort> getsCheckedForLeavesTypes,
    int hollowTreeFoliageStyle)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(getsCheckedForLeavesTypes);
    int treeHeight = 1;
    WorldTile topTile = default;
    bool foundTopTile = false;
    for (int offset = -1; offset > -100; offset--)
    {
      int tileY = y + offset;
      if (!snapshot.Metadata.IsInside(x, tileY))
      {
        break;
      }

      WorldTile tile = snapshot.GetTile(x, tileY);
      if (!tile.IsActive || !getsCheckedForLeavesTypes.Contains(tile.Type))
      {
        break;
      }

      topTile = tile;
      foundTopTile = true;
      treeHeight++;
    }

    if (!foundTopTile)
    {
      return new TreeLeafScanResult(false, treeHeight, 0, -1);
    }

    for (int offset = 1; offset < 5; offset++)
    {
      int tileY = y + offset;
      if (!snapshot.Metadata.IsInside(x, tileY))
      {
        break;
      }

      WorldTile tile = snapshot.GetTile(x, tileY);
      if (tile.IsActive && getsCheckedForLeavesTypes.Contains(tile.Type))
      {
        treeHeight++;
        continue;
      }

      TreeLeafPassStyleResult passStyle = TreeLeafPassStyleQuery.Evaluate(
        x,
        topTile,
        tile,
        treeHeight,
        hollowTreeFoliageStyle);
      return new TreeLeafScanResult(
        true,
        passStyle.TreeHeight,
        passStyle.TreeFrame,
        passStyle.PassStyle);
    }

    return new TreeLeafScanResult(true, treeHeight, 0, -1);
  }
}
