using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class Tile4x4ValidationQuery
{
  private const int TileFrameWidth = 18;

  public static Tile4x4ValidationResult Evaluate(
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
    int styleBand = frameColumn / 4;
    int frameBand = source.FrameY / 72;
    int originX = x - frameColumn % 4;
    int originY = y - source.FrameY % 72 / TileFrameWidth;
    bool valid = true;
    for (int offsetX = 0; offsetX < 4; offsetX++)
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
        valid &= tile.IsActive && tile.Type == tileType &&
          tile.FrameX == checked((short)(styleBand * 72 + offsetX * TileFrameWidth)) &&
          tile.FrameY == checked((short)(frameBand * 72 + offsetY * TileFrameWidth));
      }

      valid &= TileStateQuery.IsSolidAllowingBottomSlope(
        snapshot,
        tileDefinitions,
        originX + offsetX,
        originY + 4);
    }

    return new Tile4x4ValidationResult(
      valid,
      !valid,
      originX,
      originY,
      styleBand,
      frameBand);
  }
}
