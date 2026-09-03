using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class BannerValidationQuery
{
  private const int TileFrameWidth = 18;
  private const int BannerHeight = 3;
  private const ushort SpecialPlatformType = 380;

  public static BannerValidationResult Evaluate(
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
    int frameRow = source.FrameY / TileFrameWidth;
    int styleBand = frameRow / BannerHeight;
    int originY = y - frameRow % BannerHeight;
    bool valid = true;
    for (int offset = 0; offset < BannerHeight; offset++)
    {
      int tileY = originY + offset;
      WorldTile tile = snapshot.Metadata.IsInside(x, tileY)
        ? snapshot.GetTile(x, tileY)
        : default;
      valid &= tile.IsActive && tile.Type == tileType &&
        tile.FrameX == source.FrameX &&
        tile.FrameY == checked((short)(offset * TileFrameWidth +
          styleBand * BannerHeight * TileFrameWidth));
    }

    WorldTile support = snapshot.Metadata.IsInside(x, originY - 1)
      ? snapshot.GetTile(x, originY - 1)
      : default;
    bool hasHangingSupport = support.Type == SpecialPlatformType
      ? support.IsActive
      : TileStateQuery.IsSolidAllowingTopSlope(support, tileDefinitions);
    valid &= hasHangingSupport;
    return new BannerValidationResult(valid, !valid, originY, styleBand, hasHangingSupport);
  }
}
