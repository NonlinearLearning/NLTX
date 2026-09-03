using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class Tile3x1ValidationQuery
{
  private const int TileFrameWidth = 18;

  public static Tile3x1ValidationResult Evaluate(
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
    int styleBand = 0;
    while (frameColumn > 2)
    {
      frameColumn -= 3;
      styleBand++;
    }

    int originX = x - frameColumn;
    bool valid = source.IsActive && source.Type == tileType && source.FrameY == 0;
    for (int offset = 0; offset < 3; offset++)
    {
      int tileX = originX + offset;
      WorldTile tile = snapshot.Metadata.IsInside(tileX, y)
        ? snapshot.GetTile(tileX, y)
        : default;
      bool frameMatches = tile.IsActive && tile.Type == tileType &&
        tile.FrameX == checked((short)(offset * TileFrameWidth + styleBand * 54)) &&
        tile.FrameY == 0;
      bool supportMatches = TileStateQuery.IsSolidAllowingBottomSlope(
        snapshot,
        tileDefinitions,
        tileX,
        y + 1);
      valid &= frameMatches && supportMatches;
    }

    return new Tile3x1ValidationResult(valid, tileType == 235 && !valid, originX, y);
  }
}
