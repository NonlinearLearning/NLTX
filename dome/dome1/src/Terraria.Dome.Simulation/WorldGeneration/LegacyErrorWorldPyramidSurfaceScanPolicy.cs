using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyErrorWorldPyramidSurfaceScanPolicy
{
  public static bool TryFindFirstActivePlacementY(
    WorldGridSnapshot snapshot,
    int x,
    int candidateY,
    out int placementY)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    placementY = -1;
    if (x < 0 || x >= snapshot.Metadata.Width || candidateY < 0 ||
        candidateY >= snapshot.Metadata.Height)
    {
      return false;
    }

    for (int activeY = candidateY; activeY < snapshot.Metadata.Height; activeY++)
    {
      if (!snapshot.GetTile(x, activeY).IsActive)
      {
        continue;
      }

      if (activeY == 0)
      {
        return false;
      }

      placementY = activeY - 1;
      return true;
    }

    return false;
  }
}
