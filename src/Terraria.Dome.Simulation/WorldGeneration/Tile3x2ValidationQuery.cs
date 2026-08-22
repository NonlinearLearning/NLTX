using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class Tile3x2ValidationQuery
{
  private const int TileFrameWidth = 18;
  private const ushort PlatformTileType = 14;

  public static Tile3x2ValidationResult Evaluate(
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
    while (frameColumn > 2)
    {
      frameColumn -= 3;
      styleBand++;
    }

    int originX = x - frameColumn;
    int frameBand = source.FrameY / 36;
    int originY = y - (source.FrameY % 36) / TileFrameWidth;
    int footprintHeight = tileType == PlatformTileType && styleBand == 25 ? 1 : 2;
    bool valid = true;
    for (int offsetX = 0; offsetX < 3; offsetX++)
    {
      for (int offsetY = 0; offsetY < footprintHeight; offsetY++)
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
          tile.FrameX == checked((short)(offsetX * TileFrameWidth + styleBand * 54)) &&
          tile.FrameY == checked((short)(offsetY * TileFrameWidth + frameBand * 36));
      }

      valid &= TileStateQuery.IsSolidAllowingBottomSlope(
        snapshot,
        tileDefinitions,
        originX + offsetX,
        originY + footprintHeight);
    }

    bool deferredSpecialCase = tileType is 186 or 187 or 488 or 704 or 705 or 26 or 695;
    return new Tile3x2ValidationResult(
      valid,
      !valid,
      originX,
      originY,
      styleBand,
      footprintHeight,
      deferredSpecialCase);
  }
}
