using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class Tile4x3WallValidationQuery
{
  private const int TileFrameWidth = 18;

  public static Tile4x3WallValidationResult Evaluate(
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
    int styleBand = 0;
    while (frameRow >= 3)
    {
      frameRow -= 3;
      styleBand++;
    }

    int originX = x - frameColumn;
    int originY = y - frameRow;
    bool valid = true;
    for (int offsetX = 0; offsetX < 4; offsetX++)
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
          tile.FrameX == checked((short)(offsetX * TileFrameWidth)) &&
          tile.FrameY == checked((short)(styleBand * 54 + offsetY * TileFrameWidth));
      }
    }

    return new Tile4x3WallValidationResult(valid, !valid, originX, originY, styleBand);
  }
}
