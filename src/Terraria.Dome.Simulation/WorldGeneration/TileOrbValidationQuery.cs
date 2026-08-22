using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileOrbValidationQuery
{
  public static TileOrbValidationResult Evaluate(
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
    int originX = x - source.FrameX / 36;
    int originY = source.FrameY == 0 ? y : y - 1;
    bool valid = true;
    for (int offsetX = 0; offsetX < 2; offsetX++)
    {
      for (int offsetY = 0; offsetY < 2; offsetY++)
      {
        int tileX = originX + offsetX;
        int tileY = originY + offsetY;
        WorldTile tile = snapshot.Metadata.IsInside(tileX, tileY)
          ? snapshot.GetTile(tileX, tileY)
          : default;
        valid &= tile.IsActive && tile.Type == tileType;
      }

      if (tileType is 12 or 639)
      {
        valid &= TileStateQuery.IsSolidAllowingBottomSlope(
          snapshot,
          tileDefinitions,
          originX + offsetX,
          originY + 2);
      }
    }

    return new TileOrbValidationResult(valid, !valid, originX, originY);
  }
}
