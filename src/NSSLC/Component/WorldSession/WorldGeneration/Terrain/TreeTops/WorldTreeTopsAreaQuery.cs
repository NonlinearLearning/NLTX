using System;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Terrain.TreeTops;

public static class WorldTreeTopsAreaQuery
{
  public static void GetTileCoordinates(
    float worldPositionX,
    float worldPositionY,
    out int tileX,
    out int tileY)
  {
    tileX = (int)(worldPositionX / 16f);
    tileY = (int)(worldPositionY / 16f) + 1;
  }

  public static bool TryGetAreaId(
    bool tileActive,
    int tileType,
    int tileX,
    bool isOceanDepths,
    IReadOnlyList<int>? treeX,
    out int areaId)
  {
    areaId = -1;
    if (!tileActive)
    {
      return false;
    }

    if (tileType == 70)
    {
      areaId = WorldTreeTopsAreaId.GlowingMushroom;
    }
    else if (tileType == 53)
    {
      areaId = isOceanDepths
        ? WorldTreeTopsAreaId.Ocean
        : WorldTreeTopsAreaId.Desert;
    }
    else if (tileType == 23)
    {
      areaId = WorldTreeTopsAreaId.Corruption;
    }
    else if (tileType == 199)
    {
      areaId = WorldTreeTopsAreaId.Crimson;
    }
    else if (tileType == 109 || tileType == 492)
    {
      areaId = WorldTreeTopsAreaId.Hallow;
    }
    else if (tileType == 147)
    {
      areaId = WorldTreeTopsAreaId.Snow;
    }
    else if (tileType == 60)
    {
      areaId = WorldTreeTopsAreaId.Jungle;
    }
    else if (tileType == 633)
    {
      areaId = WorldTreeTopsAreaId.Underworld;
    }
    else if (tileType == 2 || tileType == 477)
    {
      ArgumentNullException.ThrowIfNull(treeX);
      if (treeX.Count < 3)
      {
        throw new ArgumentException(
          "Tree boundary data must contain at least three positions.",
          nameof(treeX));
      }

      areaId = tileX >= treeX[0]
        ? tileX < treeX[1]
          ? WorldTreeTopsAreaId.Forest2
          : tileX >= treeX[2]
            ? WorldTreeTopsAreaId.Forest4
            : WorldTreeTopsAreaId.Forest3
        : WorldTreeTopsAreaId.Forest1;
    }
    else
    {
      return false;
    }

    return true;
  }
}
