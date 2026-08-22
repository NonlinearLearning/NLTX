using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldObjects.Chest.Systems;

public static class LegacyChestDestructionQuery
{
  public static bool TryGetIsChestBlocked(
    WorldTile tile,
    int tileX,
    int tileY,
    IReadOnlyDictionary<int, ChestComponent> chests,
    ChestIndexSystem chestIndexSystem,
    out bool isChestBlocked)
  {
    ArgumentNullException.ThrowIfNull(chests);
    ArgumentNullException.ThrowIfNull(chestIndexSystem);
    isChestBlocked = default;
    if (!LegacyChestOriginQuery.TryGetOrigin(tile, tileX, tileY, out int originX, out int originY))
    {
      return false;
    }

    if (!chestIndexSystem.TryGetChestId(originX, originY, out int chestId))
    {
      return true;
    }

    if (!chests.TryGetValue(chestId, out ChestComponent? chest) ||
        chest.TileX != originX || chest.TileY != originY)
    {
      return false;
    }

    isChestBlocked = !ChestDestructionRuleSystem.CanDestroy(chest);
    return true;
  }
}
