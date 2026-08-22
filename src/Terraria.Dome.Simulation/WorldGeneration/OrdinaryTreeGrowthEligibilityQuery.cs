using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class OrdinaryTreeGrowthEligibilityQuery
{
  private const ushort SaplingTileType = 20;

  public static OrdinaryTreeGrowthEligibilityResult Evaluate(
    WorldGridSnapshot snapshot,
    int checkedX,
    int checkedY,
    IReadOnlyDictionary<ushort, TreeWallDefinition> wallDefinitions,
    bool ignoreWalls)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(wallDefinitions);
    if (!IsInside(snapshot, checkedX - 1, checkedY - 1) ||
        !IsInside(snapshot, checkedX + 1, checkedY - 1))
    {
      return OrdinaryTreeGrowthEligibilityResult.Rejected(
        OrdinaryTreeGrowthEligibilityReason.OutOfBounds);
    }

    int groundY = checkedY;
    while (groundY < snapshot.Metadata.Height &&
           snapshot.GetTile(checkedX, groundY) is { IsActive: true, Type: SaplingTileType })
    {
      groundY++;
    }

    if (!IsInside(snapshot, checkedX - 1, groundY - 1) ||
        !IsInside(snapshot, checkedX + 1, groundY - 1) ||
        !IsInside(snapshot, checkedX - 1, groundY) ||
        !IsInside(snapshot, checkedX + 1, groundY))
    {
      return OrdinaryTreeGrowthEligibilityResult.Rejected(
        OrdinaryTreeGrowthEligibilityReason.OutOfBounds);
    }

    if (snapshot.GetTile(checkedX - 1, groundY - 1).LiquidAmount != 0 ||
        snapshot.GetTile(checkedX, groundY - 1).LiquidAmount != 0 ||
        snapshot.GetTile(checkedX + 1, groundY - 1).LiquidAmount != 0)
    {
      return OrdinaryTreeGrowthEligibilityResult.Rejected(
        OrdinaryTreeGrowthEligibilityReason.LiquidAboveGround);
    }

    WorldTile groundTile = snapshot.GetTile(checkedX, groundY);
    if (!groundTile.IsActive)
    {
      return OrdinaryTreeGrowthEligibilityResult.Rejected(
        OrdinaryTreeGrowthEligibilityReason.GroundInactive);
    }

    if (groundTile.IsHalfBrick || groundTile.Slope != 0)
    {
      return OrdinaryTreeGrowthEligibilityResult.Rejected(
        OrdinaryTreeGrowthEligibilityReason.GroundShape);
    }

    if (!OrdinaryTreeGroundQuery.IsSuitable(groundTile.Type))
    {
      return OrdinaryTreeGrowthEligibilityResult.Rejected(
        OrdinaryTreeGrowthEligibilityReason.GroundUnsuitable);
    }

    WorldTile aboveGroundTile = snapshot.GetTile(checkedX, groundY - 1);
    if (!ignoreWalls && aboveGroundTile.WallType != 0 &&
        (!wallDefinitions.TryGetValue(aboveGroundTile.WallType, out TreeWallDefinition wall) ||
         !IsSuitableWall(wall)))
    {
      return OrdinaryTreeGrowthEligibilityResult.Rejected(
        OrdinaryTreeGrowthEligibilityReason.WallUnsuitable);
    }

    if (!IsSuitableGround(snapshot.GetTile(checkedX - 1, groundY)) &&
        !IsSuitableGround(snapshot.GetTile(checkedX + 1, groundY)))
    {
      return OrdinaryTreeGrowthEligibilityResult.Rejected(
        OrdinaryTreeGrowthEligibilityReason.NoSuitableNeighbor);
    }

    return new OrdinaryTreeGrowthEligibilityResult(
      OrdinaryTreeGrowthEligibilityReason.Eligible,
      groundY);
  }

  public static bool IsSuitableWall(TreeWallDefinition wall)
  {
    return wall.AllowsPlantsToGrow;
  }

  private static bool IsInside(WorldGridSnapshot snapshot, int x, int y)
  {
    return x >= 0 && x < snapshot.Metadata.Width && y >= 0 && y < snapshot.Metadata.Height;
  }

  private static bool IsSuitableGround(WorldTile tile)
  {
    return tile.IsActive && OrdinaryTreeGroundQuery.IsSuitable(tile.Type);
  }
}
