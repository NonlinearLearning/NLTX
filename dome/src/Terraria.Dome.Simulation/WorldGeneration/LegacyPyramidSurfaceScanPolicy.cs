using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyPyramidSurfaceScanPolicy
{
  public const ushort SandTileType = 53;

  public static bool TryFindSandPlacementY(
    WorldGridSnapshot snapshot,
    int x,
    int candidateY,
    int worldSurfaceY,
    out int placementY)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    placementY = -1;
    if (x < 0 || x >= snapshot.Metadata.Width || candidateY < 0 ||
        candidateY >= snapshot.Metadata.Height || worldSurfaceY <= 0)
    {
      return false;
    }

    int scanLimit = Math.Min(worldSurfaceY, snapshot.Metadata.Height);
    for (int activeY = candidateY; activeY < scanLimit; activeY++)
    {
      WorldTile tile = snapshot.GetTile(x, activeY);
      if (!tile.IsActive)
      {
        continue;
      }

      if (tile.Type != SandTileType || activeY == 0)
      {
        return false;
      }

      placementY = activeY - 1;
      return true;
    }

    return false;
  }
}
