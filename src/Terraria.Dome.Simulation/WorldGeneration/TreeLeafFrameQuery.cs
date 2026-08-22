using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TreeLeafFrameQuery
{
  private const ushort PalmTreeTileType = 323;

  public static bool IsLeafyTreeTop(
    WorldTile tile,
    IReadOnlySet<ushort> getsCheckedForLeavesTypes)
  {
    ArgumentNullException.ThrowIfNull(getsCheckedForLeavesTypes);
    if (!tile.IsActive || !getsCheckedForLeavesTypes.Contains(tile.Type))
    {
      return false;
    }

    if (tile.Type == PalmTreeTileType && tile.FrameX >= 88)
    {
      return true;
    }

    return tile.FrameX == 22 && tile.FrameY is >= 198 and <= 242;
  }
}
