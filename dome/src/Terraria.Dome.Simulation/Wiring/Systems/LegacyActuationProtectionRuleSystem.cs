using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Wiring.Systems;

public static class LegacyActuationProtectionRuleSystem
{
  private static readonly IReadOnlySet<ushort> ProtectedTileTypes = new HashSet<ushort>
  {
    21, 467, 26, 77, 88, 470, 475, 237, 597, 441, 468
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterDefaults()
  {
    return ProtectedTileTypes;
  }

  public static bool PreventsActuationUnder(ushort tileType)
  {
    return ProtectedTileTypes.Contains(tileType);
  }
}
