using System;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Terrain.TreeTops;

public static class WorldTreeTopsSystem
{
  private static readonly int[] SnowStyles =
  {
    0, 1, 2, 21, 22, 3, 31, 32, 4, 41, 42, 5, 6, 7
  };

  public static bool TryRandomizeTreeStyleForTile(
    WorldTreeTopsStateComponent state,
    IWorldTreeTopsRandomSource random,
    bool tileActive,
    int tileType,
    int tileX,
    bool isOceanDepths,
    IReadOnlyList<int>? treeX,
    out WorldTreeTopsStyleChangeResult result)
  {
    ArgumentNullException.ThrowIfNull(state);
    ArgumentNullException.ThrowIfNull(random);

    if (!WorldTreeTopsAreaQuery.TryGetAreaId(
          tileActive,
          tileType,
          tileX,
          isOceanDepths,
          treeX,
          out int areaId))
    {
      result = default;
      return false;
    }

    result = RandomizeTreeStyle(state, random, areaId);
    return true;
  }

  public static WorldTreeTopsStyleChangeResult RandomizeTreeStyle(
    WorldTreeTopsStateComponent state,
    IWorldTreeTopsRandomSource random,
    int areaId)
  {
    ArgumentNullException.ThrowIfNull(state);
    ArgumentNullException.ThrowIfNull(random);

    int previousStyle = state.GetTreeStyle(areaId);
    int currentStyle = previousStyle;
    int exclusiveUpperBound = GetStyleCount(areaId);
    while (currentStyle == previousStyle)
    {
      int randomIndex = random.Next(exclusiveUpperBound);
      if ((uint)randomIndex >= (uint)exclusiveUpperBound)
      {
        throw new InvalidOperationException(
          "The world TreeTops random source returned an out-of-range value.");
      }

      currentStyle = areaId == WorldTreeTopsAreaId.Snow
        ? SnowStyles[randomIndex]
        : randomIndex;
    }

    state.SetTreeStyle(areaId, currentStyle);
    return new WorldTreeTopsStyleChangeResult(
      areaId,
      previousStyle,
      currentStyle);
  }

  private static int GetStyleCount(int areaId)
  {
    return areaId switch
    {
      WorldTreeTopsAreaId.Forest1 or
      WorldTreeTopsAreaId.Forest2 or
      WorldTreeTopsAreaId.Forest3 or
      WorldTreeTopsAreaId.Forest4 or
      WorldTreeTopsAreaId.Jungle or
      WorldTreeTopsAreaId.Crimson or
      WorldTreeTopsAreaId.Ocean or
      WorldTreeTopsAreaId.Underworld => 6,
      WorldTreeTopsAreaId.Corruption or
      WorldTreeTopsAreaId.Hallow or
      WorldTreeTopsAreaId.Desert => 5,
      WorldTreeTopsAreaId.Snow => SnowStyles.Length,
      WorldTreeTopsAreaId.GlowingMushroom => 4,
      _ => throw new ArgumentOutOfRangeException(nameof(areaId))
    };
  }
}
