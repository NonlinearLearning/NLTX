using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class Tile3x5ValidationQuery
{
  private const int TileFrameWidth = 18;

  public static Tile3x5ValidationResult Evaluate(
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
    int styleBand = frameColumn / 3;
    int originX = x - frameColumn % 3;
    int originY = y - source.FrameY % 90 / TileFrameWidth;
    bool valid = true;
    for (int offsetX = 0; offsetX < 3; offsetX++)
    {
      for (int offsetY = 0; offsetY < 5; offsetY++)
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
          tile.FrameX == checked((short)(styleBand * 54 + offsetX * TileFrameWidth)) &&
          tile.FrameY == checked((short)(offsetY * TileFrameWidth));
      }

      valid &= TileStateQuery.IsSolidAllowingBottomSlope(
        snapshot,
        tileDefinitions,
        originX + offsetX,
        originY + 5);
    }

    return new Tile3x5ValidationResult(valid, !valid, originX, originY, styleBand);
  }
}
