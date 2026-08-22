using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileTypeCategoryCountQuery
{
  public static int Evaluate(
    IReadOnlyList<int> tileTypeCounts,
    TileScanGroupKind group)
  {
    ArgumentNullException.ThrowIfNull(tileTypeCounts);
    if (tileTypeCounts.Count <= 403)
    {
      throw new ArgumentException("Tile type counts must include all mapped legacy IDs.");
    }

    return group switch
    {
      TileScanGroupKind.None => 0,
      TileScanGroupKind.Corruption =>
        tileTypeCounts[23] + tileTypeCounts[24] + tileTypeCounts[25] + tileTypeCounts[32] +
        tileTypeCounts[112] + tileTypeCounts[163] + tileTypeCounts[400] + tileTypeCounts[398] -
        5 * tileTypeCounts[27],
      TileScanGroupKind.Crimson =>
        tileTypeCounts[199] + tileTypeCounts[203] + tileTypeCounts[200] + tileTypeCounts[401] +
        tileTypeCounts[399] + tileTypeCounts[234] + tileTypeCounts[352] - 5 * tileTypeCounts[27],
      TileScanGroupKind.Hallow =>
        tileTypeCounts[109] + tileTypeCounts[110] + tileTypeCounts[113] + tileTypeCounts[117] +
        tileTypeCounts[116] + tileTypeCounts[164] + tileTypeCounts[403] + tileTypeCounts[402],
      TileScanGroupKind.TotalGoodEvil =>
        Evaluate(tileTypeCounts, TileScanGroupKind.Hallow) -
        Evaluate(tileTypeCounts, TileScanGroupKind.Corruption) -
        Evaluate(tileTypeCounts, TileScanGroupKind.Crimson) + 5 * tileTypeCounts[27],
      _ => 0
    };
  }
}
