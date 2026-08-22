using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TreeProfileGrowthEligibilityQuery
{
  public static TreeProfileGrowthEligibilityResult Evaluate(
    WorldGridSnapshot snapshot,
    LegacyTreeProfileKind treeProfileKind,
    int checkedX,
    int checkedY,
    IReadOnlyDictionary<ushort, TreeGroundTileDefinition> groundDefinitions,
    IReadOnlyDictionary<ushort, TreeWallDefinition> wallDefinitions,
    bool ignoreWalls)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(groundDefinitions);
    ArgumentNullException.ThrowIfNull(wallDefinitions);
    if (!LegacyTreeProfileRegistry.TryGet(treeProfileKind, out LegacyTreeProfileDefinition profile))
    {
      return TreeProfileGrowthEligibilityResult.Rejected(
        TreeProfileGrowthEligibilityReason.UnknownProfile);
    }

    if (!IsInside(snapshot, checkedX - 1, checkedY - 1) ||
        !IsInside(snapshot, checkedX + 1, checkedY - 1))
    {
      return TreeProfileGrowthEligibilityResult.Rejected(
        TreeProfileGrowthEligibilityReason.OutOfBounds);
    }

    WorldTile initialTile = snapshot.GetTile(checkedX, checkedY);
    int groundY = checkedY;
    while (initialTile.IsActive && groundY < snapshot.Metadata.Height &&
           snapshot.GetTile(checkedX, groundY).Type == profile.SaplingTileType)
    {
      groundY++;
    }

    if (!IsInside(snapshot, checkedX - 1, groundY - 1) ||
        !IsInside(snapshot, checkedX + 1, groundY - 1) ||
        !IsInside(snapshot, checkedX - 1, groundY) ||
        !IsInside(snapshot, checkedX + 1, groundY))
    {
      return TreeProfileGrowthEligibilityResult.Rejected(
        TreeProfileGrowthEligibilityReason.OutOfBounds);
    }

    if (snapshot.GetTile(checkedX - 1, groundY - 1).LiquidAmount != 0 ||
        snapshot.GetTile(checkedX, groundY - 1).LiquidAmount != 0 ||
        snapshot.GetTile(checkedX + 1, groundY - 1).LiquidAmount != 0)
    {
      return TreeProfileGrowthEligibilityResult.Rejected(
        TreeProfileGrowthEligibilityReason.LiquidAboveGround);
    }

    WorldTile groundTile = snapshot.GetTile(checkedX, groundY);
    if (!groundTile.IsActive)
    {
      return TreeProfileGrowthEligibilityResult.Rejected(
        TreeProfileGrowthEligibilityReason.GroundInactive);
    }

    if (groundTile.IsHalfBrick || groundTile.Slope != 0)
    {
      return TreeProfileGrowthEligibilityResult.Rejected(
        TreeProfileGrowthEligibilityReason.GroundShape);
    }

    if (!IsSuitableGround(treeProfileKind, groundTile, groundDefinitions))
    {
      return TreeProfileGrowthEligibilityResult.Rejected(
        TreeProfileGrowthEligibilityReason.GroundUnsuitable);
    }

    WorldTile aboveGroundTile = snapshot.GetTile(checkedX, groundY - 1);
    if (!ignoreWalls && !IsSuitableWall(treeProfileKind, aboveGroundTile, wallDefinitions))
    {
      return TreeProfileGrowthEligibilityResult.Rejected(
        TreeProfileGrowthEligibilityReason.WallUnsuitable);
    }

    if (!IsSuitableGround(
          treeProfileKind,
          snapshot.GetTile(checkedX - 1, groundY),
          groundDefinitions) &&
        !IsSuitableGround(
          treeProfileKind,
          snapshot.GetTile(checkedX + 1, groundY),
          groundDefinitions))
    {
      return TreeProfileGrowthEligibilityResult.Rejected(
        TreeProfileGrowthEligibilityReason.NoSuitableNeighbor);
    }

    return new TreeProfileGrowthEligibilityResult(
      TreeProfileGrowthEligibilityReason.Eligible,
      groundY);
  }

  private static bool IsInside(WorldGridSnapshot snapshot, int x, int y)
  {
    return x >= 0 && x < snapshot.Metadata.Width && y >= 0 && y < snapshot.Metadata.Height;
  }

  private static bool IsSuitableGround(
    LegacyTreeProfileKind treeProfileKind,
    WorldTile tile,
    IReadOnlyDictionary<ushort, TreeGroundTileDefinition> groundDefinitions)
  {
    return tile.IsActive &&
      groundDefinitions.TryGetValue(tile.Type, out TreeGroundTileDefinition definition) &&
      TreeGroundSuitabilityQuery.IsSuitable(treeProfileKind, definition);
  }

  private static bool IsSuitableWall(
    LegacyTreeProfileKind treeProfileKind,
    WorldTile tile,
    IReadOnlyDictionary<ushort, TreeWallDefinition> wallDefinitions)
  {
    return wallDefinitions.TryGetValue(tile.WallType, out TreeWallDefinition definition) &&
      TreeWallSuitabilityQuery.IsSuitable(treeProfileKind, definition);
  }
}
