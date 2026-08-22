using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class StalactiteEchoValidationQuery
{
  private const int TileFrameWidth = 18;
  private const ushort TallEchoTileType = 694;

  public static StalactiteEchoValidationResult Evaluate(
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
    int height = source.Type == TallEchoTileType ? 2 : 1;
    bool hangsFromTop = source.FrameY < height * TileFrameWidth;
    int frameBand = source.FrameY / (height * TileFrameWidth);
    int originY = y - source.FrameY % (height * TileFrameWidth) / TileFrameWidth;
    bool valid = true;
    for (int offsetY = 0; offsetY < height; offsetY++)
    {
      int tileY = originY + offsetY;
      WorldTile tile = snapshot.Metadata.IsInside(x, tileY)
        ? snapshot.GetTile(x, tileY)
        : default;
      valid &= tile.IsActive && tile.Type == source.Type &&
        tile.FrameX == source.FrameX &&
        tile.FrameY == checked((short)(frameBand * height * TileFrameWidth +
          offsetY * TileFrameWidth));
    }

    int supportY = hangsFromTop ? originY - 1 : originY + height;
    bool hasSupport = hangsFromTop
      ? TileStateQuery.IsSolidAllowingTopSlope(snapshot, tileDefinitions, x, supportY)
      : TileStateQuery.IsSolidAllowingBottomSlope(snapshot, tileDefinitions, x, supportY);
    valid &= hasSupport;
    return new StalactiteEchoValidationResult(
      valid,
      !valid,
      originY,
      height,
      hangsFromTop,
      source.FrameX / TileFrameWidth);
  }
}
