using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldObjects;
using Terraria.Dome.Simulation.WorldObjects.Chest.Systems;

namespace Terraria.Dome.Simulation.Wiring.Systems;

public static class LegacyCanKillTileQuery
{
  public static bool TryGetCanKill(
    WorldGrid world,
    int tileX,
    int tileY,
    bool isHardMode,
    IReadOnlyDictionary<int, ChestComponent> chests,
    ChestIndexSystem chestIndexSystem,
    out bool canKill)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(chests);
    ArgumentNullException.ThrowIfNull(chestIndexSystem);
    canKill = default;
    if (!world.Contains(tileX, tileY))
    {
      return false;
    }

    WorldTile tile = world.GetTile(tileX, tileY);
    if (!tile.IsActive || tile.WallType == 350)
    {
      return false;
    }

    bool activeAboveHasProtectedSpecialRelation = false;
    bool activeAboveHasProtectedTreeRelation = false;
    if (tileY >= 1)
    {
      WorldTile aboveTile = world.GetTile(tileX, tileY - 1);
      if (aboveTile.IsActive)
      {
        activeAboveHasProtectedTreeRelation =
          TreeTrunkProtectionRuleSystem.ShouldProtectAbove(tile.Type, aboveTile);
        activeAboveHasProtectedSpecialRelation =
          SpecialTileProtectionRuleSystem.ShouldProtectAbove(tile.Type, aboveTile);
      }
    }

    bool isBoulderBlockedByChest = false;
    if (LegacyBoulderRuleSystem.IsBoulder(tile.Type) &&
        (!LegacyBoulderChestProtectionQuery.TryGetIsBlocked(
          world,
          tileX,
          tileY,
          isHardMode,
          out isBoulderBlockedByChest)))
    {
      return false;
    }

    bool isMultiTileBlocked = false;
    if (tile.Type == 235 &&
        (!LegacyMultiTileProtectionQuery.TryGetIsBlocked(
          world,
          tileX,
          tileY,
          isHardMode,
          out isMultiTileBlocked)))
    {
      return false;
    }

    bool isChestBlocked = false;
    if (tile.Type is 21 or 467 or 88 &&
        (!LegacyChestDestructionQuery.TryGetIsChestBlocked(
          tile,
          tileX,
          tileY,
          chests,
          chestIndexSystem,
          out isChestBlocked)))
    {
      return false;
    }

    bool isLockedDoor = tile.Type == 10 && LockedDoorRuleSystem.IsLocked(tile);
    canKill = ActuatorCanKillTileRuleSystem.CanKill(
      isInsideWorld: true,
      tileExists: true,
      isActive: tile.IsActive,
      wallType: tile.WallType,
      activeAboveHasProtectedTreeRelation,
      activeAboveHasProtectedSpecialRelation,
      activeAboveHasProtectedFrameRelation: false,
      isBoulderBlockedByChest,
      isLockedDoor,
      isMultiTileBlocked,
      isChestBlocked);
    return true;
  }
}
