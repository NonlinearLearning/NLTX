using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class ChestValidationQuery
{
  private const int TileFrameWidth = 18;
  private const int ChestWidth = 2;
  private const int ChestHeight = 2;

  public static ChestValidationResult Evaluate(
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
    int frameRow = source.FrameY / TileFrameWidth;
    int originX = x - frameColumn % ChestWidth;
    int originY = y - frameRow;
    bool valid = true;
    for (int offsetX = 0; offsetX < ChestWidth; offsetX++)
    {
      for (int offsetY = 0; offsetY < ChestHeight; offsetY++)
      {
        int tileX = originX + offsetX;
        int tileY = originY + offsetY;
        WorldTile tile = snapshot.Metadata.IsInside(tileX, tileY)
          ? snapshot.GetTile(tileX, tileY)
          : default;
        valid &= tile.IsActive && tile.Type == tileType &&
          tile.FrameX / TileFrameWidth % ChestWidth == offsetX &&
          tile.FrameY == checked((short)(offsetY * TileFrameWidth));
      }
    }

    bool hasSolidSupport = true;
    for (int offsetX = 0; offsetX < ChestWidth; offsetX++)
    {
      hasSolidSupport &= TileStateQuery.IsSolidAllowingBottomSlope(
        snapshot,
        tileDefinitions,
        originX + offsetX,
        originY + ChestHeight);
    }

    valid &= hasSolidSupport;
    return new ChestValidationResult(valid, !valid, originX, originY, hasSolidSupport);
  }
}
