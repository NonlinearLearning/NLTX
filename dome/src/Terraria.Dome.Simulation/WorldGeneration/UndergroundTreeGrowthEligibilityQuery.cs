using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class UndergroundTreeGrowthEligibilityQuery
{
  private const int CanopyHalfWidth = 1;
  private const int CanopyPadding = 7;
  private const int MaximumHeight = 14;
  private const int MinimumHeight = 5;
  private const ushort SaplingTileType = 20;
  private const ushort UndergroundTreeGroundTileType = 60;

  public static UndergroundTreeGrowthEligibilityResult Evaluate(
    WorldGridSnapshot snapshot,
    int originX,
    int groundY,
    int height)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (height < MinimumHeight || height > MaximumHeight)
    {
      return UndergroundTreeGrowthEligibilityResult.Rejected(
        UndergroundTreeGrowthEligibilityReason.HeightOutOfRange);
    }

    if (!snapshot.Metadata.IsInside(originX, groundY) ||
        !snapshot.Metadata.IsInside(originX - 1, groundY) ||
        !snapshot.Metadata.IsInside(originX + 1, groundY))
    {
      return UndergroundTreeGrowthEligibilityResult.Rejected(
        UndergroundTreeGrowthEligibilityReason.OutOfBounds);
    }

    WorldTile ground = snapshot.GetTile(originX, groundY);
    if (!ground.IsActive)
    {
      return UndergroundTreeGrowthEligibilityResult.Rejected(
        UndergroundTreeGrowthEligibilityReason.GroundInactive);
    }

    if (ground.Type != UndergroundTreeGroundTileType)
    {
      return UndergroundTreeGrowthEligibilityResult.Rejected(
        UndergroundTreeGrowthEligibilityReason.GroundType);
    }

    if (ground.IsHalfBrick || ground.Slope != 0)
    {
      return UndergroundTreeGrowthEligibilityResult.Rejected(
        UndergroundTreeGrowthEligibilityReason.GroundShape);
    }

    if (!IsUndergroundGround(snapshot.GetTile(originX - 1, groundY)) &&
        !IsUndergroundGround(snapshot.GetTile(originX + 1, groundY)))
    {
      return UndergroundTreeGrowthEligibilityResult.Rejected(
        UndergroundTreeGrowthEligibilityReason.NoSuitableNeighbor);
    }

    int canopyTopY = groundY - height - CanopyPadding;
    if (!TreeCanopyClearanceQuery.IsClear(
          snapshot,
          originX - CanopyHalfWidth,
          originX + CanopyHalfWidth,
          canopyTopY,
          groundY - 1,
          SaplingTileType,
          CommonSaplingTileRegistry.RegisterDefaults()))
    {
      return UndergroundTreeGrowthEligibilityResult.Rejected(
        UndergroundTreeGrowthEligibilityReason.CanopyBlocked);
    }

    return new UndergroundTreeGrowthEligibilityResult(
      UndergroundTreeGrowthEligibilityReason.Eligible,
      canopyTopY);
  }

  private static bool IsUndergroundGround(WorldTile tile)
  {
    return tile.IsActive && tile.Type == UndergroundTreeGroundTileType;
  }
}
