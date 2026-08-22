using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class Tile2x2StyleValidationQuery
{
  private const int TileFrameWidth = 18;
  private const ushort PumpkinTileType = 254;

  public static Tile2x2StyleValidationResult Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    ushort tileType)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    WorldTile source = snapshot.GetTile(x, y);
    int frameColumn = source.FrameX / TileFrameWidth;
    int styleBand = 0;
    while (frameColumn > 1)
    {
      frameColumn -= 2;
      styleBand++;
    }

    int originX = x - frameColumn;
    int frameRow = source.FrameY / TileFrameWidth;
    while (frameRow > 1)
    {
      frameRow -= 2;
    }

    int originY = y - frameRow;
    bool valid = true;
    for (int offsetX = 0; offsetX < 2; offsetX++)
    {
      for (int offsetY = 0; offsetY < 2; offsetY++)
      {
        int tileX = originX + offsetX;
        int tileY = originY + offsetY;
        if (!snapshot.Metadata.IsInside(tileX, tileY))
        {
          valid = false;
          continue;
        }

        WorldTile tile = snapshot.GetTile(tileX, tileY);
        valid &= tile.IsActive && tile.Type == tileType &&
          tile.FrameX == checked((short)(offsetX * TileFrameWidth + styleBand * 36));
      }

      bool supported = TileStateQuery.IsSolidAllowingBottomSlope(
        snapshot,
        tileDefinitions,
        originX + offsetX,
        originY + 2);
      if (tileType == PumpkinTileType && supported)
      {
        WorldTile support = snapshot.Metadata.IsInside(originX + offsetX, originY + 2)
          ? snapshot.GetTile(originX + offsetX, originY + 2)
          : default;
        supported = support.Type is 2 or 109 or 477 or 492;
      }

      valid &= supported;
    }

    return new Tile2x2StyleValidationResult(valid, !valid, originX, originY, styleBand);
  }
}
