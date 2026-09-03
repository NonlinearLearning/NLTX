using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyErrorWorldCandidateQuery
{
  private const byte ShimmerLiquidType = 3;

  public static bool IsSwapEligible(
    WorldGridSnapshot snapshot,
    IReadOnlyDictionary<ushort, LegacyErrorWorldTileDefinition> definitions,
    int x,
    int y)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(definitions);
    if (!snapshot.Metadata.IsInside(x, y - 1) || !snapshot.Metadata.IsInside(x, y + 1))
    {
      return false;
    }

    WorldTile tile = snapshot.GetTile(x, y);
    return !HasShimmer(tile) && IsSolidCandidate(tile, definitions) &&
      !IsFrameImportant(snapshot.GetTile(x, y - 1), definitions) &&
      !IsFrameImportant(snapshot.GetTile(x, y + 1), definitions);
  }

  private static bool HasShimmer(WorldTile tile)
  {
    return tile.LiquidAmount > 0 && tile.LiquidType == ShimmerLiquidType;
  }

  private static bool IsFrameImportant(
    WorldTile tile,
    IReadOnlyDictionary<ushort, LegacyErrorWorldTileDefinition> definitions)
  {
    return definitions.TryGetValue(tile.Type, out LegacyErrorWorldTileDefinition definition) &&
      definition.IsFrameImportant;
  }

  private static bool IsSolidCandidate(
    WorldTile tile,
    IReadOnlyDictionary<ushort, LegacyErrorWorldTileDefinition> definitions)
  {
    return definitions.TryGetValue(tile.Type, out LegacyErrorWorldTileDefinition definition) &&
      definition.IsSolid && !definition.IsSolidTop && !definition.IsFrameImportant;
  }
}
