using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyOrePatchTypePolicy
{
  public static ushort SelectTileType(
    ushort copperTileType,
    ushort ironTileType,
    LegacyPassRandomState random)
  {
    ArgumentNullException.ThrowIfNull(random);
    return random.Next(3) == 0 ? ironTileType : copperTileType;
  }
}
