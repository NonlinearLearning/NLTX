namespace Terraria.Dome.Simulation.Wiring.Systems;

public static class ActuatorDeactivationRuleSystem
{
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
    return tileType is 314 or 379 or 386 or 387 or 388 or 389 or 476;
  }
}
