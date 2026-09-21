using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class XmasTreeValidationQuery
{
  private const int TreeWidth = 4;
  private const int TreeHeight = 8;
  private const ushort XmasTreeTileType = 171;

  public static XmasTreeValidationResult Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    WorldTile source = snapshot.GetTile(x, y);
    int originX = source.FrameX < 10 ? x - source.FrameX : x;
    int originY = source.FrameX < 10 ? y - source.FrameY : y;
    bool valid = true;
    int activeTiles = 0;
    int previousFrameX = 0;
    for (int offsetX = 0; offsetX < TreeWidth; offsetX++)
    {
      int previousFrameY = 0;
      for (int offsetY = 0; offsetY < TreeHeight; offsetY++)
      {
        WorldTile tile = snapshot.Metadata.IsInside(originX + offsetX, originY + offsetY)
          ? snapshot.GetTile(originX + offsetX, originY + offsetY)
          : default;
        if (tile.IsActive && tile.Type == XmasTreeTileType)
        {
          activeTiles++;
        }
        else
        {
          valid = false;
        }

        if (offsetX > 0 && offsetY > 0 &&
            tile.FrameX != previousFrameX && tile.FrameY != previousFrameY)
        {
          valid = false;
        }

        previousFrameY = tile.FrameY;
      }

      previousFrameX = snapshot.Metadata.IsInside(originX + offsetX, originY)
        ? snapshot.GetTile(originX + offsetX, originY).FrameX
        : 0;
    }

    bool hasGroundSupport = true;
    for (int offsetX = 1; offsetX < TreeWidth - 1; offsetX++)
    {
      hasGroundSupport &= TileStateQuery.IsSolidAllowingBottomSlope(
        snapshot,
        tileDefinitions,
        originX + offsetX,
        originY + TreeHeight);
    }

    valid &= hasGroundSupport;
    return new XmasTreeValidationResult(
      valid,
      !valid,
      originX,
      originY,
      hasGroundSupport,
      activeTiles);
  }
}
