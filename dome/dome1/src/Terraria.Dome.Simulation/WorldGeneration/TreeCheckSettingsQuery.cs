using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TreeCheckSettingsQuery
{
  public static bool IsGroundValid(
    LegacyTreeProfileKind treeProfileKind,
    int groundTileType,
    IReadOnlyDictionary<ushort, TreeGroundTileDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    return groundTileType >= 0 && groundTileType <= ushort.MaxValue &&
      definitions.TryGetValue((ushort)groundTileType, out TreeGroundTileDefinition definition) &&
      TreeGroundSuitabilityQuery.IsSuitable(treeProfileKind, definition);
  }

  public static bool IsWallValid(
    LegacyTreeProfileKind treeProfileKind,
    int wallType,
    IReadOnlyDictionary<ushort, TreeWallDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    return wallType >= 0 && wallType <= ushort.MaxValue &&
      definitions.TryGetValue((ushort)wallType, out TreeWallDefinition definition) &&
      TreeWallSuitabilityQuery.IsSuitable(treeProfileKind, definition);
  }
}
