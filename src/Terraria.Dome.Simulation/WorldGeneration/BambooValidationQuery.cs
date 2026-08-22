using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class BambooValidationQuery
{
  private const ushort BambooTileType = 571;
  private const ushort BaseTileType = 60;

  public static BambooValidationResult Evaluate(
    WorldGridSnapshot snapshot,
    int x,
    int y)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    WorldTile plant = snapshot.GetTile(x, y);
    WorldTile support = GetTile(snapshot, x, y + 1);
    WorldTile above = GetTile(snapshot, x, y - 1);
    bool hasSupport = support.IsActive && support.Type is BaseTileType or BambooTileType;
    bool hasBambooAbove = above.IsActive && above.Type == BambooTileType;
    int frameBand = plant.FrameX / 18;
    bool validFrame = plant.FrameY == 0 &&
      ((!hasSupport && !hasBambooAbove && frameBand == 0) ||
       (hasBambooAbove && hasSupport && frameBand is >= 5 and <= 14) ||
       (hasBambooAbove && !hasSupport && frameBand is >= 1 and <= 4) ||
       (!hasBambooAbove && hasSupport && frameBand is >= 15 and <= 19));
    bool valid = plant.IsActive && hasSupport && validFrame;
    return new BambooValidationResult(valid, !valid, hasSupport, hasBambooAbove, frameBand);
  }

  private static WorldTile GetTile(WorldGridSnapshot snapshot, int x, int y)
  {
    return snapshot.Metadata.IsInside(x, y) ? snapshot.GetTile(x, y) : default;
  }
}
