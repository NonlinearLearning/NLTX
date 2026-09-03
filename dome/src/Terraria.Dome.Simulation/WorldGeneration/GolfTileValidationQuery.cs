using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class GolfTileValidationQuery
{
  private const int TileFrameWidth = 18;

  public static GolfTileValidationResult Evaluate(
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

    WorldTile tile = snapshot.GetTile(x, y);
    bool hasAlignedFrame = tile.FrameX % TileFrameWidth == 0 &&
      tile.FrameY % TileFrameWidth == 0;
    bool hasSolidSupport = snapshot.Metadata.IsInside(x, y + 1) &&
      TileStateQuery.IsSolidAllowingBottomSlope(snapshot, tileDefinitions, x, y + 1);
    bool valid = hasAlignedFrame && hasSolidSupport;
    return new GolfTileValidationResult(valid, !valid, hasAlignedFrame, hasSolidSupport);
  }
}
