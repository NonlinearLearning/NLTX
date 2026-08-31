using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyErrorWorldRandomBlock
{
  private const ushort FirstExcludedTileType = 58;
  private const ushort SecondExcludedTileType = 226;
  private const ushort ThirdExcludedTileType = 404;

  public static bool TrySelect(
    IReadOnlyList<LegacyErrorWorldTileDefinition> tileDefinitions,
    LegacyPassRandomState random,
    out ushort tileType)
  {
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    ArgumentNullException.ThrowIfNull(random);
    tileType = default;
    if (!HasEligibleTile(tileDefinitions))
    {
      return false;
    }

    while (true)
    {
      LegacyErrorWorldTileDefinition definition = tileDefinitions[random.Next(tileDefinitions.Count)];
      if (IsEligible(definition))
      {
        tileType = definition.TileType;
        return true;
      }
    }
  }

  private static bool HasEligibleTile(IReadOnlyList<LegacyErrorWorldTileDefinition> tileDefinitions)
  {
    foreach (LegacyErrorWorldTileDefinition definition in tileDefinitions)
    {
      if (IsEligible(definition))
      {
        return true;
      }
    }

    return false;
  }

  private static bool IsEligible(LegacyErrorWorldTileDefinition definition)
  {
    return definition.IsSolid && !definition.IsSolidTop && !definition.IsFrameImportant &&
      !definition.IsDungeon && definition.TileType is not FirstExcludedTileType and
      not SecondExcludedTileType and not ThirdExcludedTileType;
  }
}
