using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class Tile1xXValidationQuery
{
  private const int TileFrameWidth = 18;

  public static Tile1xXValidationResult Evaluate(
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

    int height = tileType == 92 ? 6 : 3;
    WorldTile source = snapshot.GetTile(x, y);
    int frameRow = source.FrameY / TileFrameWidth;
    int frameBand = frameRow / height;
    int originY = y - frameRow % height;
    bool valid = true;
    for (int offsetY = 0; offsetY < height; offsetY++)
    {
      int tileY = originY + offsetY;
      if (!snapshot.Metadata.IsInside(x, tileY))
      {
        valid = false;
        continue;
      }

      WorldTile tile = snapshot.GetTile(x, tileY);
      valid &= tile.IsActive && tile.Type == tileType &&
        tile.FrameX == source.FrameX &&
        tile.FrameY == checked(
          (short)(offsetY * TileFrameWidth + frameBand * height * TileFrameWidth));
    }

    valid &= TileStateQuery.IsSolidAllowingBottomSlope(
      snapshot,
      tileDefinitions,
      x,
      originY + height);

    return new Tile1xXValidationResult(valid, !valid, originY, height, frameBand);
  }
}
