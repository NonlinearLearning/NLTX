using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyTileRunnerPassInput(
  string PassName,
  string RecipeName,
  int TileType,
  bool AddTile,
  int MinimumStrength,
  int MaximumStrengthExclusive,
  int MinimumSteps,
  int MaximumStepsExclusive,
  string VerticalRange,
  bool UsesRandomX,
  bool UsesRandomY,
  bool ResetsRandomFromWorldSeed);

public static class LegacyTileRunnerPassInputDefinition
{
  private static readonly IReadOnlyList<LegacyTileRunnerPassInput> DefaultRecipes =
    Array.AsReadOnly<LegacyTileRunnerPassInput>(
    [
      new LegacyTileRunnerPassInput(
        "DirtLayerCaves", "surface-dirt", 1, false, 4, 15, 5, 40,
        "0..worldSurfaceLow", true, true, true),
      new LegacyTileRunnerPassInput(
        "DirtLayerCaves", "surface-high-dirt", 1, false, 4, 10, 5, 30,
        "worldSurfaceLow..worldSurfaceHigh", true, true, true),
      new LegacyTileRunnerPassInput(
        "DirtLayerCaves", "rock-high-dirt", 1, false, 2, 7, 2, 23,
        "worldSurfaceHigh..rockLayerHigh", true, true, true),
      new LegacyTileRunnerPassInput(
        "RockLayerCaves", "rock-layer-stone", 0, false, 2, 6, 2, 40,
        "rockLayerLow..maxTilesY", true, true, true),
      new LegacyTileRunnerPassInput(
        "SurfaceCaves", "surface-desert", 40, false, 4, 14, 10, 50,
        "0..worldSurfaceLow", true, true, true)
    ]);

  public static IReadOnlyList<LegacyTileRunnerPassInput> CreateDefaultRecipes()
  {
    return DefaultRecipes;
  }

  public static void Validate(IReadOnlyCollection<LegacyTileRunnerPassInput> recipes)
  {
    ArgumentNullException.ThrowIfNull(recipes);
    HashSet<string> keys = new(StringComparer.Ordinal);
    foreach (LegacyTileRunnerPassInput recipe in recipes)
    {
      if (recipe is null || string.IsNullOrWhiteSpace(recipe.PassName) ||
          string.IsNullOrWhiteSpace(recipe.RecipeName) ||
          !keys.Add(recipe.PassName + ":" + recipe.RecipeName) ||
          recipe.MinimumStrength < 0 ||
          recipe.MaximumStrengthExclusive <= recipe.MinimumStrength ||
          recipe.MinimumSteps < 0 ||
          recipe.MaximumStepsExclusive <= recipe.MinimumSteps ||
          string.IsNullOrWhiteSpace(recipe.VerticalRange) ||
          !recipe.ResetsRandomFromWorldSeed)
      {
        throw new ArgumentException("TileRunner pass recipe contract was invalid.", nameof(recipes));
      }
    }
  }
}
