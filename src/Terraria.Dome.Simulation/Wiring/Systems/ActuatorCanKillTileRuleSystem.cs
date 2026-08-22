namespace Terraria.Dome.Simulation.Wiring.Systems;

public static class ActuatorCanKillTileRuleSystem
{
  public static bool CanKill(
    bool isInsideWorld,
    bool tileExists,
    bool isActive,
    ushort wallType,
    bool activeAboveHasProtectedTreeRelation,
    bool activeAboveHasProtectedSpecialRelation,
    bool activeAboveHasProtectedFrameRelation,
    bool isBoulderBlockedByChest,
    bool isLockedDoor,
    bool isMultiTileBlocked,
    bool isChestBlocked)
  {
    if (!isInsideWorld || !tileExists || !isActive || wallType == 350)
    {
      return false;
    }

    if (activeAboveHasProtectedTreeRelation || activeAboveHasProtectedSpecialRelation ||
        activeAboveHasProtectedFrameRelation || isBoulderBlockedByChest || isLockedDoor ||
        isMultiTileBlocked || isChestBlocked)
    {
      return false;
    }

    return true;
  }
}
