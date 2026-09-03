using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyUnderworldEvilRunnerFactory
{
  public static IReadOnlyList<LegacyTileRunnerRequest> Create(
    LegacyUnderworldLiquidCaveOrigin origin,
    LegacyPassRandomState random)
  {
    ArgumentNullException.ThrowIfNull(random);
    List<LegacyTileRunnerRequest> requests = new(3)
    {
      CreateRequest(origin, random, -10, 10, -10, 10, 5, 15, 5, 10)
    };

    if (random.Next(3) == 0)
    {
      requests.Add(CreateRequest(origin, random, -10, 10, -10, 10, 10, 30, 10, 20));
    }

    if (random.Next(5) == 0)
    {
      requests.Add(CreateRequest(origin, random, -15, 15, -15, 10, 15, 30, 5, 20));
    }

    return requests.AsReadOnly();
  }

  private static LegacyTileRunnerRequest CreateRequest(
    LegacyUnderworldLiquidCaveOrigin origin,
    LegacyPassRandomState random,
    int minimumOffset,
    int maximumOffsetExclusive,
    int minimumYOffset,
    int maximumYOffsetExclusive,
    int minimumStrength,
    int maximumStrengthExclusive,
    int minimumSteps,
    int maximumStepsExclusive)
  {
    return LegacyTileRunnerRequest.CreateLegacyUnboundedStart(
      origin.X + random.Next(minimumOffset, maximumOffsetExclusive),
      origin.Y + random.Next(minimumYOffset, maximumYOffsetExclusive),
      random.Next(minimumStrength, maximumStrengthExclusive),
      random.Next(minimumSteps, maximumStepsExclusive),
      -2,
      addTile: false,
      random.Next(-1, 3),
      random.Next(-1, 3),
      noYChange: false,
      overwrite: false,
      ignoreTileType: -1);
  }
}
