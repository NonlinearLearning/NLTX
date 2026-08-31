using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileTypeCategoryCountQuery
{
  private static readonly IReadOnlyDictionary<TileScanGroupKind, IReadOnlyList<int>> DefaultGroups =
    new Dictionary<TileScanGroupKind, IReadOnlyList<int>>
    {
      [TileScanGroupKind.Corruption] = Array.AsReadOnly(
        new[] { 23, 24, 25, 32, 112, 163, 400, 398 }),
      [TileScanGroupKind.Crimson] = Array.AsReadOnly(
        new[] { 199, 203, 200, 401, 399, 234, 352 }),
      [TileScanGroupKind.Hallow] = Array.AsReadOnly(
        new[] { 109, 110, 113, 117, 116, 164, 403, 402 })
    }.ToFrozenDictionary();

  public static IReadOnlyDictionary<TileScanGroupKind, IReadOnlyList<int>> RegisterDefaults()
  {
    return DefaultGroups;
  }

  public static int Evaluate(
    IReadOnlyList<int> tileTypeCounts,
    TileScanGroupKind group,
    bool remixWorld = false)
  {
    ArgumentNullException.ThrowIfNull(tileTypeCounts);
    if (tileTypeCounts.Count <= 403)
    {
      throw new ArgumentException("Tile type counts must include all mapped legacy IDs.");
    }

    if (remixWorld && tileTypeCounts.Count <= 474)
    {
      throw new ArgumentException(
        "Remix tile type counts must include the legacy remix alignment IDs.",
        nameof(tileTypeCounts));
    }

    int remixCorruption = remixWorld ? tileTypeCounts[474] : 0;
    int remixCrimson = remixWorld ? tileTypeCounts[195] : 0;
    return group switch
    {
      TileScanGroupKind.None => 0,
      TileScanGroupKind.Corruption => Sum(tileTypeCounts, TileScanGroupKind.Corruption) -
        5 * tileTypeCounts[27] + remixCorruption,
      TileScanGroupKind.Crimson => Sum(tileTypeCounts, TileScanGroupKind.Crimson) -
        5 * tileTypeCounts[27] + remixCrimson,
      TileScanGroupKind.Hallow => Sum(tileTypeCounts, TileScanGroupKind.Hallow),
      TileScanGroupKind.TotalGoodEvil =>
        Evaluate(tileTypeCounts, TileScanGroupKind.Hallow, remixWorld) -
        Evaluate(tileTypeCounts, TileScanGroupKind.Corruption, remixWorld) -
        Evaluate(tileTypeCounts, TileScanGroupKind.Crimson, remixWorld) +
        5 * tileTypeCounts[27],
      _ => 0
    };
  }

  private static int Sum(IReadOnlyList<int> tileTypeCounts, TileScanGroupKind group)
  {
    int total = 0;
    IReadOnlyList<int> tileTypes = DefaultGroups[group];
    for (int index = 0; index < tileTypes.Count; index++)
    {
      total += tileTypeCounts[tileTypes[index]];
    }

    return total;
  }
}
