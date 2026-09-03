using System.Collections.Frozen;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Wiring.Systems;

public static class TileBreakabilityProtectionRuleSystem
{
  private const ushort DemonAltarTileType = 77;

  private static readonly IReadOnlySet<ushort> _containerTileTypes = new HashSet<ushort>
  {
    21, 88, 441, 467, 468, 470, 475
  }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> _preventsRemovalOnTopTileTypes = new HashSet<ushort>
  {
    5, 26, 72, 323, 470, 475, 488, 583, 584, 585, 586, 587, 588, 589, 596, 616, 634
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterContainerDefaults()
  {
    return _containerTileTypes;
  }

  public static IReadOnlySet<ushort> RegisterTopRemovalDefaults()
  {
    return _preventsRemovalOnTopTileTypes;
  }

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
