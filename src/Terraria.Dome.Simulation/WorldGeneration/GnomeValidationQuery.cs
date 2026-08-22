using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class GnomeValidationQuery
{
  private const ushort GnomeTileType = 567;

  public static GnomeValidationResult Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    ushort type = GnomeTileType)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    WorldTile currentTile = snapshot.GetTile(x, y);
    int originY = currentTile.FrameY > 0 ? y - 1 : y;
    bool insideFootprint = snapshot.Metadata.IsInside(x, originY + 1);
    bool expectedFootprint = insideFootprint &&
      IsExpectedTile(snapshot.GetTile(x, originY), type, frameY: 0) &&
      IsExpectedTile(snapshot.GetTile(x, originY + 1), type, frameY: 20);
    bool supportedGround = snapshot.Metadata.IsInside(x, originY + 2) &&
      IsSupportedGround(snapshot, tileDefinitions, x, originY + 2);
    bool isValid = expectedFootprint && supportedGround;
    return new GnomeValidationResult(
      isValid,
      !isValid,
      originY,
      expectedFootprint,
      supportedGround,
      true);
  }

  private static bool IsExpectedTile(WorldTile tile, ushort type, short frameY)
  {
    return tile.IsActive && tile.Type == type && tile.FrameY == frameY;
  }

  private static bool IsSupportedGround(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y)
  {
    WorldTile tile = snapshot.GetTile(x, y);
    if (tileDefinitions.TryGet(tile.Type, out TileDefinition definition) &&
        definition.IsPlatform)
    {
      return tile.IsActive && !tile.IsInactive;
    }

    return TileStateQuery.IsSolidAllowingBottomSlope(snapshot, tileDefinitions, x, y);
  }
}
