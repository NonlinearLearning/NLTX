using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LogicTileValidationQuery
{
  private const int TileFrameWidth = 18;
  private const ushort LogicTileType = 419;
  private const ushort LogicTileSupportType = 420;

  public static LogicTileValidationResult Evaluate(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    ushort tileType)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    WorldTile tile = snapshot.GetTile(x, y);
    bool hasAlignedFrame = tile.FrameX % TileFrameWidth == 0 &&
      tile.FrameY % TileFrameWidth == 0;
    bool hasRequiredLogicSupport = true;
    if (tileType == LogicTileType)
    {
      WorldTile support = snapshot.Metadata.IsInside(x, y + 1)
        ? snapshot.GetTile(x, y + 1)
        : default;
      hasRequiredLogicSupport = support.IsActive &&
        (support.Type == LogicTileType || support.Type == LogicTileSupportType);
    }

    bool valid = hasAlignedFrame && hasRequiredLogicSupport;
    return new LogicTileValidationResult(
      valid,
      !valid,
      hasAlignedFrame,
      hasRequiredLogicSupport);
  }
}
