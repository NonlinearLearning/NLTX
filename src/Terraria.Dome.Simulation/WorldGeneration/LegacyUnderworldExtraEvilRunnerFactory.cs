using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyUnderworldExtraEvilRunnerFactory
{
  public static int CalculateCount(int width, bool isDrunkWorld, bool isRemixWorld)
  {
    if (width <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    return isDrunkWorld || isRemixWorld ? checked(width * 2) : 0;
  }

  public static IReadOnlyList<LegacyTileRunnerRequest> Create(
    int width,
    int height,
    LegacyPassRandomState random,
    bool isDrunkWorld,
    bool isRemixWorld)
  {
    ArgumentNullException.ThrowIfNull(random);
    if (width <= 0 || height <= 180)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    int count = CalculateCount(width, isDrunkWorld, isRemixWorld);
    List<LegacyTileRunnerRequest> requests = new(count);
    for (int index = 0; index < count; index++)
    {
      requests.Add(new LegacyTileRunnerRequest(
        random.Next((int)(width * 0.35), (int)(width * 0.65)),
        random.Next(height - 180, height - 10),
        random.Next(5, 20),
        random.Next(5, 10),
        -2,
        addTile: false,
        speedX: 0.0,
        speedY: 0.0,
        noYChange: false,
        overwrite: false,
        ignoreTileType: -1));
    }

    return requests.AsReadOnly();
  }
}
