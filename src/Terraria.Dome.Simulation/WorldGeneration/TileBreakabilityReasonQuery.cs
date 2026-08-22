using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.Wiring.Systems;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileBreakabilityReasonQuery
{
  public static bool HasReasonToReturnEarly(
    ushort ignoredTileType,
    WorldTile targetTile,
    bool isHardMode,
    bool scanForContainer)
  {
    return TileBreakabilityProtectionRuleSystem.HasReasonToReturnEarly(
      ignoredTileType,
      targetTile,
      isHardMode,
      scanForContainer);
  }
}
