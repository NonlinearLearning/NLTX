using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.Wiring.Systems;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class BoulderChestValidationQuery
{
  public static BoulderChestValidationResult Evaluate(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    bool isHardMode = false)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    WorldTile boulder = snapshot.GetTile(x, y);
    bool isBoulder = LegacyBoulderRuleSystem.IsBoulder(boulder.Type);
    if (!BoulderChestProtectionRuleSystem.TryGetAboveCoordinates(
      boulder,
      x,
      y,
      out int leftAboveX,
      out int aboveY,
      out int rightAboveX))
    {
      return new BoulderChestValidationResult(isBoulder, false, x, y, y - 1, false);
    }

    bool hasProtectedContainer = false;
    if (snapshot.Metadata.IsInside(leftAboveX, aboveY))
    {
      hasProtectedContainer |= TileBreakabilityProtectionRuleSystem.HasReasonToReturnEarly(
        boulder.Type,
        snapshot.GetTile(leftAboveX, aboveY),
        isHardMode,
        scanForContainer: true);
    }

    if (snapshot.Metadata.IsInside(rightAboveX, aboveY))
    {
      hasProtectedContainer |= TileBreakabilityProtectionRuleSystem.HasReasonToReturnEarly(
        boulder.Type,
        snapshot.GetTile(rightAboveX, aboveY),
        isHardMode,
        scanForContainer: true);
    }

    return new BoulderChestValidationResult(
      isBoulder,
      isBoulder && hasProtectedContainer,
      leftAboveX,
      y - (boulder.FrameY / 18),
      aboveY,
      hasProtectedContainer);
  }
}
