using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class Tile2xXValidationQuery
{
  private const int TileFrameWidth = 18;

  public static Tile2xXValidationResult Evaluate(
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

    int height = tileType switch
    {
      104 => 5,
      207 => 4,
      _ => 3
    };
    WorldTile source = snapshot.GetTile(x, y);
    int originX = x - source.FrameX / TileFrameWidth % 2;
    int frameRow = source.FrameY / TileFrameWidth;
    int frameBand = frameRow / height;
    int originY = y - frameRow % height;
    bool valid = true;
    for (int offsetY = 0; offsetY < height; offsetY++)
    {
      for (int offsetX = 0; offsetX < 2; offsetX++)
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
          tile.FrameX == checked((short)(offsetX * TileFrameWidth + source.FrameX / 36 * 36)) &&
          tile.FrameY == checked(
            (short)(offsetY * TileFrameWidth + frameBand * height * TileFrameWidth));
      }
    }

    bool usesTopSupport = tileType is 465 or 531 or 591 or 592;
    if (usesTopSupport)
    {
      valid &= TileStateQuery.IsSolidAllowingTopSlope(
        snapshot,
        tileDefinitions,
        originX,
        originY - 1);
      valid &= TileStateQuery.IsSolidAllowingTopSlope(
        snapshot,
        tileDefinitions,
        originX + 1,
        originY - 1);
    }
    else
    {
      valid &= TileStateQuery.IsSolidAllowingBottomSlope(
        snapshot,
        tileDefinitions,
        originX,
        originY + height);
      valid &= TileStateQuery.IsSolidAllowingBottomSlope(
        snapshot,
        tileDefinitions,
        originX + 1,
        originY + height);
    }

    return new Tile2xXValidationResult(
      valid,
      !valid,
      originX,
      originY,
      height,
      frameBand,
      usesTopSupport);
  }
}
