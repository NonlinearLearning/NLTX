using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class PlatformSupportQuery
{
  public static bool IsBelowANonHammeredPlatform(
    WorldTile tile,
    IReadOnlyCollection<ushort> platformTypes)
  {
    if (!tile.IsActive || tile.IsHalfBrick || tile.Slope != 0)
    {
      return false;
    }

    foreach (ushort platformType in platformTypes)
    {
      if (platformType == tile.Type)
      {
        return true;
      }
    }

    return false;
  }
}
