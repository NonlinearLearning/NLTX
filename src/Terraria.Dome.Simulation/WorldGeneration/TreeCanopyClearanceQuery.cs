using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TreeCanopyClearanceQuery
{
  private static readonly IReadOnlySet<ushort> PlantTileExceptions = new ushort[]
  {
    3,
    24,
    32,
    61,
    62,
    69,
    71,
    73,
    74,
    82,
    83,
    84,
    110,
    113,
    184,
    201,
    233,
    352,
    485,
    529,
    530,
    637,
    655
  }.ToFrozenSet<ushort>();

  public static IReadOnlySet<ushort> RegisterPlantExceptionDefaults()
  {
    return PlantTileExceptions;
  }

  public static bool IsClear(
    WorldGridSnapshot snapshot,
    int startX,
    int endX,
    int startY,
    int endY,
    int ignoreId)
  {
    return IsClear(
      snapshot,
      startX,
      endX,
      startY,
      endY,
      ignoreId,
      CommonSaplingTileRegistry.RegisterDefaults());
  }

  public static bool IsClear(
    WorldGridSnapshot snapshot,
    int startX,
    int endX,
    int startY,
    int endY,
    int ignoreId,
    IReadOnlySet<ushort> commonSaplingTypes)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(commonSaplingTypes);
    if (startX < 0 || endX >= snapshot.Metadata.Width || startY < 0 ||
        endY >= snapshot.Metadata.Height || endX < startX || endY < startY)
    {
      return false;
    }

    bool ignoreCommonSaplings = ignoreId != -1 &&
      commonSaplingTypes.Contains((ushort)ignoreId);
    for (int x = startX; x <= endX; x++)
    {
      for (int y = startY; y <= endY; y++)
      {
        WorldTile tile = snapshot.GetTile(x, y);
        if (!tile.IsActive)
        {
          continue;
        }

        if (ignoreId == -1)
        {
          return false;
        }

        if (ignoreId is 11 or 71)
        {
          if (tile.Type != (ushort)ignoreId)
          {
            return false;
          }

          continue;
        }

        if (ignoreCommonSaplings &&
            (commonSaplingTypes.Contains(tile.Type) || PlantTileExceptions.Contains(tile.Type)))
        {
          continue;
        }

        return false;
      }
    }

    return true;
  }
}
