using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TrapDoorValidationQuery
{
  private const int TileFrameWidth = 18;
  private const ushort TrapDoorType = 387;
  private const ushort TallTrapDoorType = 386;

  public static TrapDoorValidationResult Evaluate(
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

    int width = tileType == TrapDoorType ? 2 : tileType == TallTrapDoorType ? 2 : 0;
    int height = tileType == TrapDoorType ? 1 : tileType == TallTrapDoorType ? 2 : 0;
    if (width == 0)
    {
      return new TrapDoorValidationResult(false, true, x, y, 0, 0, 0, false);
    }

    WorldTile source = snapshot.GetTile(x, y);
    int frameColumn = source.FrameX / TileFrameWidth;
    int frameRow = source.FrameY / TileFrameWidth;
    int style = frameColumn;
    int originX = x - frameColumn % width;
    int originY = y - frameRow % height;
    bool valid = true;
    for (int offsetX = 0; offsetX < width; offsetX++)
    {
      for (int offsetY = 0; offsetY < height; offsetY++)
      {
        int tileX = originX + offsetX;
        int tileY = originY + offsetY;
        WorldTile tile = snapshot.Metadata.IsInside(tileX, tileY)
          ? snapshot.GetTile(tileX, tileY)
          : default;
        valid &= tile.IsActive && tile.Type == tileType &&
          tile.FrameX / TileFrameWidth % width == offsetX &&
          tile.FrameY / TileFrameWidth % height == offsetY;
      }
    }

    bool hasSolidAnchor = tileType == TrapDoorType
      ? HasSupport(snapshot, tileDefinitions, originX, originY + height, width)
      : style == 0
        ? HasSupport(snapshot, tileDefinitions, originX, originY + 1, width)
        : HasSupport(snapshot, tileDefinitions, originX, originY - 1, width);
    valid &= hasSolidAnchor;
    return new TrapDoorValidationResult(
      valid,
      !valid,
      originX,
      originY,
      width,
      height,
      style,
      hasSolidAnchor);
  }

  private static bool HasSupport(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry definitions,
    int originX,
    int supportY,
    int width)
  {
    for (int offsetX = 0; offsetX < width; offsetX++)
    {
      if (!TileStateQuery.IsSolidAllowingBottomSlope(
        snapshot,
        definitions,
        originX + offsetX,
        supportY))
      {
        return false;
      }
    }

    return true;
  }
}
