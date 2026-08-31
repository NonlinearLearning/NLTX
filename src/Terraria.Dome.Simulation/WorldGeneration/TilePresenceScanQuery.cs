using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TilePresenceScanQuery
{
  private const int ScanBorder = 40;

  public static TilePresenceScanResult Scan(WorldGridSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    int startX = Math.Min(ScanBorder, snapshot.Metadata.Width);
    int endExclusiveX = Math.Max(startX, snapshot.Metadata.Width - ScanBorder);
    int startY = Math.Min(ScanBorder, snapshot.Metadata.Height);
    int endExclusiveY = Math.Max(startY, snapshot.Metadata.Height - ScanBorder);
    HashSet<ushort> activeTileTypes = new();
    HashSet<ushort> wallTypes = new();
    int activeTileCount = 0;

    for (int x = startX; x < endExclusiveX; x++)
    {
      for (int y = startY; y < endExclusiveY; y++)
      {
        WorldTile tile = snapshot.GetTile(x, y);
        if (tile.IsActive)
        {
          activeTileCount++;
          activeTileTypes.Add(tile.Type);
        }

        wallTypes.Add(tile.WallType);
      }
    }

    return new TilePresenceScanResult(
      activeTileTypes.ToFrozenSet(),
      wallTypes.ToFrozenSet(),
      activeTileCount,
      checked(snapshot.Metadata.Width * snapshot.Metadata.Height),
      startX,
      endExclusiveX,
      startY,
      endExclusiveY);
  }
}
