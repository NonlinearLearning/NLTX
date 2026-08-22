using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class StalactiteValidationQuery
{
  private const int TileFrameWidth = 18;

  public static StalactiteValidationResult Evaluate(
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
    int originY = y;
    int height;
    bool hangsFromTop;
    if (source.FrameY == 72)
    {
      height = 1;
      hangsFromTop = true;
    }
    else if (source.FrameY == 90)
    {
      height = 1;
      hangsFromTop = false;
    }
    else if (source.FrameY >= 36)
    {
      height = 2;
      hangsFromTop = false;
      if (source.FrameY == 54)
      {
        originY--;
      }
    }
    else
    {
      height = 2;
      hangsFromTop = true;
      if (source.FrameY == 18)
      {
        originY--;
      }
    }

    bool valid = true;
    for (int offsetY = 0; offsetY < height; offsetY++)
    {
      int tileY = originY + offsetY;
      WorldTile tile = snapshot.Metadata.IsInside(x, tileY)
        ? snapshot.GetTile(x, tileY)
        : default;
      valid &= tile.IsActive && tile.Type == source.Type &&
        tile.FrameX == source.FrameX &&
        tile.FrameY == source.FrameY + offsetY * TileFrameWidth;
    }

    int supportY = hangsFromTop ? originY - 1 : originY + height;
    bool hasSupport = hangsFromTop
      ? TileStateQuery.IsSolidAllowingBottomSlope(snapshot, tileDefinitions, x, supportY)
      : TileStateQuery.IsSolidAllowingTopSlope(snapshot, tileDefinitions, x, supportY);
    valid &= hasSupport;
    return new StalactiteValidationResult(valid, !valid, originY, height, hangsFromTop, hasSupport);
  }
}
