using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyTileRunnerPassLoopDefinition(
  string PassName,
  string RecipeName,
  double TileDensity,
  double RemixMultiplier)
{
  public int CalculateInvocationCount(int width, int height, bool remixWorld)
  {
    if (width <= 0 || height <= 0 || !double.IsFinite(TileDensity) ||
        TileDensity < 0 || !double.IsFinite(RemixMultiplier) || RemixMultiplier < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    double count = (double)width * height * TileDensity;
    if (remixWorld)
    {
      count *= RemixMultiplier;
    }

    return checked((int)Math.Ceiling(count));
  }
}

public static class LegacyTileRunnerPassLoopContractDefinition
{
  public static IReadOnlyList<LegacyTileRunnerPassLoopDefinition> CreateDefault()
  {
    return new[]
    {
      new LegacyTileRunnerPassLoopDefinition(
        "DirtLayerCaves", "surface-dirt", 0.00015, 1.0),
      new LegacyTileRunnerPassLoopDefinition(
        "DirtLayerCaves", "surface-high-dirt", 0.0002, 1.0),
      new LegacyTileRunnerPassLoopDefinition(
        "DirtLayerCaves", "rock-high-dirt", 0.0045, 1.0)
    };
  }
}
