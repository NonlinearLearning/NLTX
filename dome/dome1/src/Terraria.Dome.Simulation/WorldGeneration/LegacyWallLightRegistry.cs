using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyWallLightRegistry
{
  private static readonly IReadOnlySet<ushort> LightWallTypes = new HashSet<ushort>
  {
    0, 21, 106, 107, 138, 139, 140, 141, 145, 150, 152, 168, 245, 315, 317, 318
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterDefaults()
  {
    return LightWallTypes;
  }

  public static bool IsLightWall(ushort wallType)
  {
    return LightWallTypes.Contains(wallType);
  }
}
