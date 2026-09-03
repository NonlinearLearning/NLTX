using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyUnderworldLiquidCaveScheduler
{
  public static IReadOnlyList<LegacyTileRunnerRequest> CreateRequests(
    WorldGridSnapshot snapshot,
    LegacyPassRandomState random,
    bool isDrunkWorld = false,
    bool isRemixWorld = false,
    bool isSkyblockWorld = false)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(random);
    if (isSkyblockWorld)
    {
      return Array.Empty<LegacyTileRunnerRequest>();
    }

    List<LegacyTileRunnerRequest> requests = new();
    for (int column = 0; column < snapshot.Metadata.Width; column++)
    {
      if (!LegacyUnderworldLiquidCaveOriginPolicy.TrySelect(
            snapshot,
            random,
            column,
            isDrunkWorld,
            isRemixWorld,
            out LegacyUnderworldLiquidCaveOrigin origin))
      {
        continue;
      }

      LegacyTileRunnerRequest? surfaceRequest =
        LegacyUnderworldSurfaceRunnerFactory.TryCreate(origin, random);
      if (surfaceRequest is not null)
      {
        requests.Add(surfaceRequest);
      }

      requests.AddRange(LegacyUnderworldPairedSurfaceRunnerFactory.Create(
        origin,
        random,
        isDrunkWorld,
        isRemixWorld));
      requests.AddRange(LegacyUnderworldEvilRunnerFactory.Create(origin, random));
    }

    return requests.AsReadOnly();
  }
}
