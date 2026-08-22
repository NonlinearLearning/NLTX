using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Wiring.Systems;

public static class TileBreakabilityProtectionRuleSystem
{
  private static readonly HashSet<ushort> _containerTileTypes =
  [
    21, 88, 441, 467, 468, 470, 475
  ];

  private static readonly HashSet<ushort> _preventsRemovalOnTopTileTypes =
  [
    5, 26, 72, 323, 470, 475, 488, 583, 584, 585, 586, 587, 588, 589, 596, 616, 634
  ];

  private const ushort DemonAltarTileType = 77;

  public static bool HasReasonToReturnEarly(
    ushort ignoredTileType,
    WorldTile targetTile,
    bool isHardMode,
    bool scanForContainer)
  {
    if (ignoredTileType != targetTile.Type)
    {
      if (targetTile.Type == DemonAltarTileType && !isHardMode)
      {
        return true;
      }

      if (_preventsRemovalOnTopTileTypes.Contains(targetTile.Type))
      {
        return true;
      }
    }

    if (LockedDoorRuleSystem.IsLocked(targetTile))
    {
      return true;
    }

    return scanForContainer && _containerTileTypes.Contains(targetTile.Type);
  }
}
