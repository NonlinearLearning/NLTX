using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class Tile6x3ValidationQuery
{
  private const int TileFrameWidth = 18;

  public static Tile6x3ValidationResult Evaluate(
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
    int originX = x - source.FrameX / TileFrameWidth;
    int originY = y - source.FrameY / TileFrameWidth;
    bool valid = true;
    for (int offsetX = 0; offsetX < 6; offsetX++)
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
        valid &= tile.IsActive && tile.Type == tileType &&
          tile.FrameX == checked((short)(offsetX * TileFrameWidth)) &&
          tile.FrameY == checked((short)(offsetY * TileFrameWidth));
      }

      valid &= TileStateQuery.IsSolidAllowingBottomSlope(
        snapshot,
        tileDefinitions,
        originX + offsetX,
        originY + 3);
    }

    return new Tile6x3ValidationResult(valid, !valid, originX, originY);
  }
}
