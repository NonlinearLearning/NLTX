using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyUnderworldLowerScheduler
{
  public static IReadOnlyList<LegacyTileRunnerRequest> CreateRequests(
    int width,
    int height,
    LegacyPassRandomState random,
    bool isDrunkWorld = false,
    bool isRemixWorld = false,
    bool isSkyblockWorld = false)
  {
    ArgumentNullException.ThrowIfNull(random);
    if (width <= 40 || height <= 180)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    if (isSkyblockWorld)
    {
      return Array.Empty<LegacyTileRunnerRequest>();
    }

    List<LegacyTileRunnerRequest> requests = new(width + width * 2);
    for (int index = 0; index < width; index++)
    {
      requests.Add(LegacyUnderworldLowerEvilRunnerFactory.Create(width, height, random));
    }

    requests.AddRange(LegacyUnderworldExtraEvilRunnerFactory.Create(
      width,
      height,
      random,
      isDrunkWorld,
      isRemixWorld));
    for (int index = 0; index < LegacyUnderworldObsidianRunnerFactory.CalculateCount(width, height);
         index++)
    {
      requests.Add(LegacyUnderworldObsidianRunnerFactory.Create(width, height, random));
    }

    return requests.AsReadOnly();
  }
}
