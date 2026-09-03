using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyTileRunnerTileState(
  bool IsActive,
  int TileType,
  bool IsFrameImportant,
  bool IsTileCut,
  bool IsProtectedDungeonTile = false);

public static class LegacyTileRunnerCandidateQuery
{
  public static bool ShouldMutate(
    LegacyTileRunnerTileState tile,
    int tileX,
    int tileY,
    double centerX,
    double centerY,
    double strength,
    int randomOffset,
    int ignoreTileType)
  {
    ArgumentNullException.ThrowIfNull(tile);
    if (tile.IsActive && tile.IsFrameImportant && !tile.IsTileCut)
    {
      return false;
    }

    if (ignoreTileType >= 0 && tile.IsActive && tile.TileType == ignoreTileType)
    {
      return false;
    }

    return LegacyTileRunnerDistancePolicy.IsWithinManhattanEnvelope(
      tileX,
      tileY,
      centerX,
      centerY,
      strength,
      randomOffset);
  }
}
