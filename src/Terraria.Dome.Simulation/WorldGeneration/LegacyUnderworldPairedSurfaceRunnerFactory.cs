using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyUnderworldPairedSurfaceRunnerFactory
{
  public static IReadOnlyList<LegacyTileRunnerRequest> Create(
    LegacyUnderworldLiquidCaveOrigin origin,
    LegacyPassRandomState random,
    bool isDrunkWorld,
    bool isRemixWorld)
  {
    ArgumentNullException.ThrowIfNull(random);
    double scale = random.Next(1, 3);
    if (random.Next(3) == 0)
    {
      scale *= 0.5;
    }

    if (!origin.SurfaceRunnersAllowed)
    {
      return Array.Empty<LegacyTileRunnerRequest>();
    }

    List<LegacyTileRunnerRequest> requests = new(2);
    if (random.Next(2) == 0)
    {
      requests.Add(CreateRequest(origin, random, scale, speedX: 1.0));
    }

    if (random.Next(2) == 0)
    {
      scale = random.Next(1, 3);
      requests.Add(CreateRequest(origin, random, scale, speedX: -1.0));
    }

    return requests.AsReadOnly();
  }

  private static LegacyTileRunnerRequest CreateRequest(
    LegacyUnderworldLiquidCaveOrigin origin,
    LegacyPassRandomState random,
    double scale,
    double speedX)
  {
    return new LegacyTileRunnerRequest(
      origin.X,
      origin.Y - random.Next(2, 5),
      (int)(random.Next(5, 15) * (double)scale),
      (int)(random.Next(10, 15) * (double)scale),
      57,
      addTile: true,
      speedX,
      speedY: 0.3,
      noYChange: false,
      overwrite: false,
      ignoreTileType: -1);
  }
}
