using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyPyramidPreMutationEligibilityPolicy
{
  private const int PyramidTileType = 151;
  private const int PyramidWallType = 151;
  private const int SandstonePyramidTileType = 203;
  private const int EvilTileType = 25;
  private const int DungeonBrickTileType = 41;
  private const int DungeonBrickTileTypeAlternate = 43;
  private const int DungeonBrickTileTypeLarge = 44;
  private const int NearbyTileDistance = 100;
  private const int PotentialBoundsFluff = 5;

  public static LegacyPyramidPreMutationDecision Evaluate(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    int pyramidMaxDepth,
    bool surfaceIsDesert,
    bool errorWorld,
    bool dualDungeonsEnabled,
    IReadOnlyList<DungeonBoundsSnapshot>? potentialDungeonBounds)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pyramidMaxDepth);
    if (x < 0 || x >= snapshot.Metadata.Width)
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    if (y < 0 || y >= snapshot.Metadata.Height)
    {
      throw new ArgumentOutOfRangeException(nameof(y));
    }

    WorldTile originTile = snapshot.GetTile(x, y);
    if (originTile.IsActive &&
        (originTile.Type == PyramidTileType || originTile.WallType == PyramidWallType))
    {
      return Reject(LegacyPyramidPreMutationRejectionReason.ExistingPyramidTileOrWall);
    }

    if (dualDungeonsEnabled)
    {
      if (potentialDungeonBounds is null || potentialDungeonBounds.Count == 0)
      {
        return Reject(LegacyPyramidPreMutationRejectionReason.PotentialDungeonBounds);
      }

      long potentialBoundsY = (long)y + pyramidMaxDepth;
      if (potentialBoundsY > int.MaxValue || potentialBoundsY < int.MinValue ||
          IsInsidePotentialDungeonBounds(x, (int)potentialBoundsY, potentialDungeonBounds))
      {
        return Reject(LegacyPyramidPreMutationRejectionReason.PotentialDungeonBounds);
      }
    }

    if (surfaceIsDesert || errorWorld)
    {
      if (TileNeighborhoodQuery.IsTileNearby(
            snapshot,
            x,
            y,
            PyramidTileType,
            NearbyTileDistance))
      {
        return Reject(LegacyPyramidPreMutationRejectionReason.NearbyPyramidTile);
      }

      if (TileNeighborhoodQuery.IsTileNearby(
            snapshot,
            x,
            y,
            SandstonePyramidTileType,
            NearbyTileDistance))
      {
        return Reject(LegacyPyramidPreMutationRejectionReason.NearbySandstonePyramidTile);
      }

      if (TileNeighborhoodQuery.IsTileNearby(
            snapshot,
            x,
            y,
            EvilTileType,
            NearbyTileDistance))
      {
        return Reject(LegacyPyramidPreMutationRejectionReason.NearbyEvilTile);
      }
    }

    if (surfaceIsDesert || errorWorld || dualDungeonsEnabled)
    {
      if (TileNeighborhoodQuery.IsTileNearby(
            snapshot,
            x,
            y,
            DungeonBrickTileType,
            NearbyTileDistance) ||
          TileNeighborhoodQuery.IsTileNearby(
            snapshot,
            x,
            y,
            DungeonBrickTileTypeAlternate,
            NearbyTileDistance) ||
          TileNeighborhoodQuery.IsTileNearby(
            snapshot,
            x,
            y,
            DungeonBrickTileTypeLarge,
            NearbyTileDistance))
      {
        return Reject(LegacyPyramidPreMutationRejectionReason.NearbyDungeonBrickTile);
      }
    }

    return new LegacyPyramidPreMutationDecision(
      LegacyPyramidPreMutationRejectionReason.None);
  }

  private static LegacyPyramidPreMutationDecision Reject(
    LegacyPyramidPreMutationRejectionReason reason)
  {
    return new LegacyPyramidPreMutationDecision(reason);
  }

  private static bool IsInsidePotentialDungeonBounds(
    int x,
    int y,
    IReadOnlyList<DungeonBoundsSnapshot> potentialDungeonBounds)
  {
    for (int index = 0; index < potentialDungeonBounds.Count; index++)
    {
      if (potentialDungeonBounds[index].ContainsWithFluff(x, y, PotentialBoundsFluff))
      {
        return true;
      }
    }

    return false;
  }
}
