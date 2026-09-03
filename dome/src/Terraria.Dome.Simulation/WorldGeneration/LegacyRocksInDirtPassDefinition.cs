using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyRocksInDirtPassDefinition
{
  private static readonly IReadOnlyList<LegacyTileRunnerPassInput> DefaultRecipes =
    Array.AsReadOnly<LegacyTileRunnerPassInput>(
    [
      new LegacyTileRunnerPassInput(
        "RocksInDirt", "surface-dirt", 1, false, 4, 15, 5, 40,
        "0..worldSurfaceLow", true, true, true),
      new LegacyTileRunnerPassInput(
        "RocksInDirt", "surface-high-dirt", 1, false, 4, 10, 5, 30,
        "worldSurfaceLow..worldSurfaceHigh", true, true, true),
      new LegacyTileRunnerPassInput(
        "RocksInDirt", "rock-high-dirt", 1, false, 2, 7, 2, 23,
        "worldSurfaceHigh..rockLayerHigh", true, true, true)
    ]);

  private static readonly IReadOnlyList<LegacyTileRunnerPassLoopDefinition> DefaultLoops =
    Array.AsReadOnly<LegacyTileRunnerPassLoopDefinition>(
    [
      new LegacyTileRunnerPassLoopDefinition("RocksInDirt", "surface-dirt", 0.00015, 1.0),
      new LegacyTileRunnerPassLoopDefinition(
        "RocksInDirt", "surface-high-dirt", 0.0002, 1.0),
      new LegacyTileRunnerPassLoopDefinition("RocksInDirt", "rock-high-dirt", 0.0045, 1.0)
    ]);

  public static IReadOnlyList<LegacyTileRunnerPassInput> CreateDefaultRecipes()
  {
    return DefaultRecipes;
  }

  public static IReadOnlyList<LegacyTileRunnerPassLoopDefinition> CreateDefaultLoops()
  {
    return DefaultLoops;
  }

  public static int CalculateInvocationCount(int width, int height, double density)
  {
    if (width <= 0 || height <= 0 || !double.IsFinite(density) || density < 0.0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    return checked((int)Math.Ceiling(width * (double)height * density));
  }
}
