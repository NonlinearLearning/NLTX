using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Wiring.Systems;

public static class MultiTileProtectionRuleSystem
{
  private const ushort MultiTileType = 235;
  private const int RequiredFootprintWidth = 3;

  public static bool IsBlocked(
    ushort candidateTileType,
    IReadOnlyList<WorldTile> aboveTiles,
    bool isHardMode)
  {
    if (candidateTileType != MultiTileType || aboveTiles.Count != RequiredFootprintWidth)
    {
      return false;
    }

    for (int index = 0; index < aboveTiles.Count; index++)
    {
      WorldTile aboveTile = aboveTiles[index];
      if (aboveTile.IsActive && TileBreakabilityProtectionRuleSystem.HasReasonToReturnEarly(
            candidateTileType,
            aboveTile,
            isHardMode,
            scanForContainer: true))
      {
        return true;
      }
    }

    return false;
  }
}
