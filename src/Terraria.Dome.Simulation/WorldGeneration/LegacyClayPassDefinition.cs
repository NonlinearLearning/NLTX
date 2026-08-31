using System;
using System.Collections.Generic;
using System.Linq;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyClayPassDefinition
{
  public const int TargetTileType = 40;
  public const double SurfaceLowDensity = 2E-05;
  public const double RemixDensity = 7E-05;
  public const double SurfaceHighDensity = 5E-05;
  public const double RockHighDensity = 2E-05;

  private static readonly IReadOnlyList<LegacyTileRunnerPassInput> DefaultRecipes =
    Array.AsReadOnly<LegacyTileRunnerPassInput>(
    [
      new LegacyTileRunnerPassInput(
        "Clay", "surface-low-clay", TargetTileType, false, 4, 14, 10, 50,
        "0..worldSurfaceLow", true, true, true),
      new LegacyTileRunnerPassInput(
        "Clay", "remix-clay", TargetTileType, false, 8, 15, 5, 50,
        "rockLayer-25..maxTilesY-350", true, true, true),
      new LegacyTileRunnerPassInput(
        "Clay", "surface-high-clay", TargetTileType, false, 8, 14, 15, 45,
        "worldSurfaceLow..worldSurfaceHigh+1", true, true, true),
      new LegacyTileRunnerPassInput(
        "Clay", "rock-high-clay", TargetTileType, false, 8, 15, 5, 50,
        "worldSurfaceHigh..rockLayerHigh+1", true, true, true)
    ]);

  private static readonly IReadOnlyList<LegacyTileRunnerPassLoopDefinition> DefaultLoops =
    Array.AsReadOnly<LegacyTileRunnerPassLoopDefinition>(
    [
      new LegacyTileRunnerPassLoopDefinition(
        "Clay", "surface-low-clay", SurfaceLowDensity, 1.0),
      new LegacyTileRunnerPassLoopDefinition(
        "Clay", "remix-clay", RemixDensity, 1.0),
      new LegacyTileRunnerPassLoopDefinition(
        "Clay", "surface-high-clay", SurfaceHighDensity, 1.0),
      new LegacyTileRunnerPassLoopDefinition(
        "Clay", "rock-high-clay", RockHighDensity, 1.0)
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

    double count = width * (double)height * density;
    return checked((int)count);
  }

  public static void Validate()
  {
    IReadOnlyList<LegacyTileRunnerPassInput> recipes = CreateDefaultRecipes();
    IReadOnlyList<LegacyTileRunnerPassLoopDefinition> loops = CreateDefaultLoops();
    if (recipes.Count != 4 || loops.Count != 4 ||
        recipes[0] != new LegacyTileRunnerPassInput(
          "Clay", "surface-low-clay", TargetTileType, false, 4, 14, 10, 50,
          "0..worldSurfaceLow", true, true, true) ||
        recipes[1] != new LegacyTileRunnerPassInput(
          "Clay", "remix-clay", TargetTileType, false, 8, 15, 5, 50,
          "rockLayer-25..maxTilesY-350", true, true, true) ||
        recipes[2] != new LegacyTileRunnerPassInput(
          "Clay", "surface-high-clay", TargetTileType, false, 8, 14, 15, 45,
          "worldSurfaceLow..worldSurfaceHigh+1", true, true, true) ||
        recipes[3] != new LegacyTileRunnerPassInput(
          "Clay", "rock-high-clay", TargetTileType, false, 8, 15, 5, 50,
          "worldSurfaceHigh..rockLayerHigh+1", true, true, true) ||
        loops[0].TileDensity != SurfaceLowDensity ||
        loops[1].TileDensity != RemixDensity ||
        loops[2].TileDensity != SurfaceHighDensity ||
        loops[3].TileDensity != RockHighDensity ||
        loops.Any(loop => loop.PassName != "Clay" || loop.RemixMultiplier != 1.0))
    {
      throw new InvalidOperationException(
        "Clay pass definition contains an invalid source contract.");
    }

    LegacyTileRunnerPassInputDefinition.Validate(recipes);
  }
}
