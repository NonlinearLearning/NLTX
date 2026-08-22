using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Wiring.Systems;

public static class LegacyBoulderChestProtectionQuery
{
  public static bool TryGetIsBlocked(
    WorldGrid world,
    int tileX,
    int tileY,
    bool isHardMode,
    out bool isBlocked)
  {
    ArgumentNullException.ThrowIfNull(world);
    isBlocked = default;
    if (!world.Contains(tileX, tileY))
    {
      return false;
    }

    WorldTile boulderTile = world.GetTile(tileX, tileY);
    if (!boulderTile.IsActive || !LegacyBoulderRuleSystem.IsBoulder(boulderTile.Type) ||
        !BoulderChestProtectionRuleSystem.TryGetAboveCoordinates(
          boulderTile,
          tileX,
          tileY,
          out int leftAboveX,
          out int aboveY,
          out int rightAboveX) ||
        !world.Contains(leftAboveX, aboveY) || !world.Contains(rightAboveX, aboveY))
    {
      return false;
    }

    bool leftBlocked = TileBreakabilityProtectionRuleSystem.HasReasonToReturnEarly(
      boulderTile.Type,
      world.GetTile(leftAboveX, aboveY),
      isHardMode,
      scanForContainer: true);
    bool rightBlocked = TileBreakabilityProtectionRuleSystem.HasReasonToReturnEarly(
      boulderTile.Type,
      world.GetTile(rightAboveX, aboveY),
      isHardMode,
      scanForContainer: true);
    isBlocked = BoulderChestProtectionRuleSystem.IsBlocked(
      isBoulder: true,
      leftAboveHasBreakabilityBlock: leftBlocked,
      rightAboveHasBreakabilityBlock: rightBlocked);
    return true;
  }
}
