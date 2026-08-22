using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class Tile6x4WallValidationQuery
{
  private const int TileFrameWidth = 18;

  public static Tile6x4WallValidationResult Evaluate(
    WorldGridSnapshot snapshot,
    int x,
    int y)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    WorldTile source = snapshot.GetTile(x, y);
    int frameColumn = source.FrameX / TileFrameWidth;
    int frameRow = source.FrameY / TileFrameWidth;
    int styleBand = frameColumn / 6;
    int frameBand = styleBand % 27;
    int frameGroup = frameColumn / 6;
    int originX = x - frameColumn % 6;
    int originY = y - frameRow % 4;
    bool valid = true;
    for (int offsetX = 0; offsetX < 6; offsetX++)
    {
      for (int offsetY = 0; offsetY < 4; offsetY++)
      {
        int tileX = originX + offsetX;
        int tileY = originY + offsetY;
        if (!snapshot.Metadata.IsInside(tileX, tileY))
        {
          valid = false;
          continue;
        }

        WorldTile tile = snapshot.GetTile(tileX, tileY);
        valid &= tile.IsActive && tile.Type == source.Type && tile.WallType > 0 &&
          tile.FrameX == checked((short)(frameGroup * 108 + offsetX * TileFrameWidth)) &&
          tile.FrameY == checked((short)(frameBand * 72 + offsetY * TileFrameWidth));
      }
    }

    return new Tile6x4WallValidationResult(
      valid,
      !valid,
      originX,
      originY,
      frameGroup,
      frameBand);
  }
}
