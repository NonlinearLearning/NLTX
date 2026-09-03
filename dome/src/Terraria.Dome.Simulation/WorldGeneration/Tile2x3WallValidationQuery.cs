using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class Tile2x3WallValidationQuery
{
  private const int TileFrameWidth = 18;

  public static Tile2x3WallValidationResult Evaluate(
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
    int styleBand = frameColumn / 2;
    int originX = x - frameColumn % 2;
    int originY = y - source.FrameY / TileFrameWidth;
    bool valid = true;
    for (int offsetX = 0; offsetX < 2; offsetX++)
    {
      for (int offsetY = 0; offsetY < 3; offsetY++)
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
          tile.FrameX == checked((short)(styleBand * 36 + offsetX * TileFrameWidth)) &&
          tile.FrameY == checked((short)(offsetY * TileFrameWidth));
      }
    }

    return new Tile2x3WallValidationResult(valid, !valid, originX, originY, styleBand);
  }
}
