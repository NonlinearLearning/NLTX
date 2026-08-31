using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyErrorWorldReplacementCandidateQuery
{
  public static bool IsEligible(
    WorldTile tile,
    IReadOnlyDictionary<ushort, LegacyErrorWorldTileDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    return definitions.TryGetValue(tile.Type, out LegacyErrorWorldTileDefinition definition) &&
      definition.IsSolid && !definition.IsSolidTop && !definition.IsFrameImportant &&
      !definition.IsDungeon;
  }
}
