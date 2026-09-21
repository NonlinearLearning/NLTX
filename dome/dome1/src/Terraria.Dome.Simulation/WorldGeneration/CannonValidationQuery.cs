using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class CannonValidationQuery
{
  private const int TileFrameWidth = 18;
  private const int CannonWidth = 4;
  private const int CannonHeight = 3;

  public static CannonValidationResult Evaluate(
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
    int styleBand = frameColumn / CannonWidth;
    int originX = x - frameColumn % CannonWidth;
    int frameRow = source.FrameY / TileFrameWidth;
    int frameBand = frameRow / CannonHeight;
    int originY = y - frameRow % CannonHeight;
    bool valid = true;
    for (int offsetX = 0; offsetX < CannonWidth; offsetX++)
    {
      for (int offsetY = 0; offsetY < CannonHeight; offsetY++)
      {
        int tileX = originX + offsetX;
        int tileY = originY + offsetY;
        WorldTile tile = snapshot.Metadata.IsInside(tileX, tileY)
          ? snapshot.GetTile(tileX, tileY)
          : default;
        valid &= tile.IsActive && tile.Type == tileType &&
          tile.FrameX == checked((short)(styleBand * 72 + offsetX * TileFrameWidth)) &&
          tile.FrameY == checked((short)(frameBand * 54 + offsetY * TileFrameWidth));
      }
    }

    bool hasInternalSupport = true;
    for (int offsetX = 1; offsetX < CannonWidth - 1; offsetX++)
    {
      hasInternalSupport &= TileStateQuery.IsSolidAllowingBottomSlope(
        snapshot,
        tileDefinitions,
        originX + offsetX,
        originY + CannonHeight);
    }

    valid &= hasInternalSupport;
    return new CannonValidationResult(
      valid,
      !valid,
      originX,
      originY,
      styleBand,
      hasInternalSupport);
  }
}
