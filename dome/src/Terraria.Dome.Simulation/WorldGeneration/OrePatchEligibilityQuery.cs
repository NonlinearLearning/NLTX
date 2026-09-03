using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class OrePatchEligibilityQuery
{
  private const int SupportBottomOffset = 30;
  private const int SupportHalfWidth = 10;
  private const int SupportTopOffset = 7;

  public static OrePatchEligibilityResult Evaluate(
    WorldGridSnapshot snapshot,
    int originX,
    int originY,
    int worldSurfaceY,
    IReadOnlyDictionary<ushort, OrePatchTileDefinition> tileDefinitions)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    if (!snapshot.Metadata.IsInside(originX - 1, originY) ||
        !snapshot.Metadata.IsInside(originX + 1, originY) ||
        worldSurfaceY < 0 || worldSurfaceY >= snapshot.Metadata.Height)
    {
      return OrePatchEligibilityResult.Rejected(OrePatchEligibilityReason.OutOfBounds);
    }

    int groundY = originY;
    while (groundY <= worldSurfaceY &&
           !IsSolid(snapshot.GetTile(originX, groundY), tileDefinitions))
    {
      groundY++;
    }

    if (groundY > worldSurfaceY)
    {
      return OrePatchEligibilityResult.Rejected(
        OrePatchEligibilityReason.NoSolidGroundBeforeSurface);
    }

    if (!snapshot.Metadata.IsInside(originX - SupportHalfWidth, groundY + SupportTopOffset) ||
        !snapshot.Metadata.IsInside(originX + SupportHalfWidth, groundY + SupportBottomOffset))
    {
      return OrePatchEligibilityResult.Rejected(OrePatchEligibilityReason.OutOfBounds);
    }

    WorldTile center = snapshot.GetTile(originX, groundY);
    if (!IsGrass(center, tileDefinitions) ||
        !IsGrass(snapshot.GetTile(originX - 1, groundY), tileDefinitions) ||
        !IsGrass(snapshot.GetTile(originX + 1, groundY), tileDefinitions))
    {
      return OrePatchEligibilityResult.Rejected(OrePatchEligibilityReason.GroundNotGrass);
    }

    if (center.WallType != 0)
    {
      return OrePatchEligibilityResult.Rejected(OrePatchEligibilityReason.GroundWallPresent);
    }

    for (int x = originX - SupportHalfWidth; x <= originX + SupportHalfWidth; x++)
    {
      for (int y = groundY + SupportTopOffset; y <= groundY + SupportBottomOffset; y++)
      {
        WorldTile tile = snapshot.GetTile(x, y);
        if (!tile.IsActive)
        {
          return OrePatchEligibilityResult.Rejected(OrePatchEligibilityReason.SupportInactive);
        }

        if (!tileDefinitions.TryGetValue(tile.Type, out OrePatchTileDefinition definition))
        {
          return OrePatchEligibilityResult.Rejected(OrePatchEligibilityReason.SupportInactive);
        }

        if (definition.IsDungeon)
        {
          return OrePatchEligibilityResult.Rejected(OrePatchEligibilityReason.SupportDungeon);
        }

        if (definition.IsCloud)
        {
          return OrePatchEligibilityResult.Rejected(OrePatchEligibilityReason.SupportCloud);
        }

        if (definition.IsSand)
        {
          return OrePatchEligibilityResult.Rejected(OrePatchEligibilityReason.SupportSand);
        }

        if (tile.WallType == 0)
        {
          return OrePatchEligibilityResult.Rejected(OrePatchEligibilityReason.SupportWallMissing);
        }
      }
    }

    return new OrePatchEligibilityResult(OrePatchEligibilityReason.Eligible, groundY);
  }

  private static bool IsGrass(
    WorldTile tile,
    IReadOnlyDictionary<ushort, OrePatchTileDefinition> tileDefinitions)
  {
    return tileDefinitions.TryGetValue(tile.Type, out OrePatchTileDefinition definition) &&
      definition.IsGrass;
  }

  private static bool IsSolid(
    WorldTile tile,
    IReadOnlyDictionary<ushort, OrePatchTileDefinition> tileDefinitions)
  {
    return tile.IsActive &&
      tileDefinitions.TryGetValue(tile.Type, out OrePatchTileDefinition definition) &&
      definition.IsSolid;
  }
}
