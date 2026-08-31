using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyMountainCaveOpeningScheduler
{
  public static IReadOnlyList<LegacyMountainCaveOpeningRequest> CreateRequests(
    IReadOnlyList<LegacyCaveCoordinate> caveHistory,
    LegacyPassRandomState random,
    bool isSkyblockWorld = false)
  {
    ArgumentNullException.ThrowIfNull(caveHistory);
    ArgumentNullException.ThrowIfNull(random);
    if (isSkyblockWorld)
    {
      return Array.Empty<LegacyMountainCaveOpeningRequest>();
    }

    List<LegacyMountainCaveOpeningRequest> requests = new(caveHistory.Count);
    for (int index = 0; index < caveHistory.Count; index++)
    {
      LegacyCaveCoordinate coordinate = caveHistory[index];
      requests.Add(new LegacyMountainCaveOpeningRequest(
        coordinate.X,
        coordinate.Y,
        random.Next(40, 50)));
    }

    return requests.AsReadOnly();
  }
}
