using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Wiring.Systems;

public static class ActuatorDeactivationRuleSystem
{
  private static readonly IReadOnlySet<ushort> SpecialNonActuatedTileTypes = new HashSet<ushort>
  {
    314, 379, 386, 387, 388, 389, 476
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterSpecialNonActuatedDefaults()
  {
    return SpecialNonActuatedTileTypes;
  }

  public static bool ShouldDeactivate(
    bool isActive,
    bool isActuated,
    ushort tileType,
    bool isSolid,
    bool isNotReallySolid,
    bool isType226,
    bool belowWorldSurface,
    bool defeatedPlantera,
    bool tileAboveIsActive,
    bool tileAbovePreventsActuation,
    bool canKillTile)
  {
    if (!isActive || !isActuated || (isType226 && belowWorldSurface && !defeatedPlantera))
    {
      return false;
    }

    if (!isSolid || isNotReallySolid || IsSpecialNonActuatedType(tileType))
    {
      return false;
    }

    return !tileAboveIsActive || (!tileAbovePreventsActuation && canKillTile);
  }

  private static bool IsSpecialNonActuatedType(ushort tileType)
  {
    return SpecialNonActuatedTileTypes.Contains(tileType);
  }
}
