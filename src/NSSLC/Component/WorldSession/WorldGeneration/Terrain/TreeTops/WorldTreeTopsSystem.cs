using System;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Terrain.TreeTops;

public static class WorldTreeTopsSystem
{
  private static readonly int[] ForestStyles =
  {
    0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 31, 51, 71, 72, 73
  };

  private static readonly int[] CorruptionStyles = { 0, 1, 2, 3, 4, 51, 52 };
  private static readonly int[] JungleStyles = { 0, 1, 2, 3, 4, 5, 6 };
  private static readonly int[] SnowStyles =
  {
    0, 1, 2, 21, 22, 3, 31, 32, 4, 41, 42, 5, 6, 7, 8
  };
  private static readonly int[] HallowStyles = { 0, 1, 2, 3, 4, 5 };
  private static readonly int[] CrimsonStyles = { 0, 1, 2, 3, 4, 5, 6 };
  private static readonly int[] DesertStyles = { 0, 1, 2, 3, 4, 51, 52, 53 };
  private static readonly int[] OceanStyles = { 0, 1, 2, 3, 4, 5, 6, 7 };
  private static readonly int[] GlowingMushroomStyles = { 0, 1, 2, 3, 4 };
  private static readonly int[] UnderworldStyles = { 0, 1, 2 };

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
    int[] styles = GetStyles(areaId);
    int exclusiveUpperBound = styles.Length;
    while (currentStyle == previousStyle)
    {
      int randomIndex = random.Next(exclusiveUpperBound);
      if ((uint)randomIndex >= (uint)exclusiveUpperBound)
      {
        throw new InvalidOperationException(
          "The world TreeTops random source returned an out-of-range value.");
      }

      currentStyle = styles[randomIndex];
    }

    state.SetTreeStyle(areaId, currentStyle);
    return new WorldTreeTopsStyleChangeResult(
      areaId,
      previousStyle,
      currentStyle);
  }

  public static bool IsValidStyle(int areaId, int style)
  {
    if ((uint)areaId >= WorldTreeTopsAreaId.Count)
    {
      return false;
    }

    return Array.IndexOf(GetStyles(areaId), style) >= 0;
  }

  public static void ApplyStyles(
    WorldTreeTopsStateComponent state,
    IReadOnlyList<int> styles)
  {
    ArgumentNullException.ThrowIfNull(state);
    ArgumentNullException.ThrowIfNull(styles);
    if (styles.Count > state.AreaCount)
    {
      throw new ArgumentException(
        "Tree tops style updates cannot exceed the number of world areas.",
        nameof(styles));
    }

    int[] stableStyles = new int[styles.Count];
    for (int index = 0; index < stableStyles.Length; index++)
    {
      int style = styles[index];
      if (!IsValidStyle(index, style))
      {
        throw new ArgumentOutOfRangeException(
          nameof(styles),
          style,
          $"Tree tops area {index} does not support style {style}.");
      }

      stableStyles[index] = style;
    }

    for (int areaId = 0; areaId < stableStyles.Length; areaId++)
    {
      state.SetTreeStyle(areaId, stableStyles[areaId]);
    }
  }

  private static int[] GetStyles(int areaId)
  {
    return areaId switch
    {
      WorldTreeTopsAreaId.Forest1 or
      WorldTreeTopsAreaId.Forest2 or
      WorldTreeTopsAreaId.Forest3 or
      WorldTreeTopsAreaId.Forest4 => ForestStyles,
      WorldTreeTopsAreaId.Corruption => CorruptionStyles,
      WorldTreeTopsAreaId.Jungle => JungleStyles,
      WorldTreeTopsAreaId.Snow => SnowStyles,
      WorldTreeTopsAreaId.Hallow => HallowStyles,
      WorldTreeTopsAreaId.Crimson => CrimsonStyles,
      WorldTreeTopsAreaId.Desert => DesertStyles,
      WorldTreeTopsAreaId.Ocean => OceanStyles,
      WorldTreeTopsAreaId.GlowingMushroom => GlowingMushroomStyles,
      WorldTreeTopsAreaId.Underworld => UnderworldStyles,
      _ => throw new ArgumentOutOfRangeException(nameof(areaId))
    };
  }
}
