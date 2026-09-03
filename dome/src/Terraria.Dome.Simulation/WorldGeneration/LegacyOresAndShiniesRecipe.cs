using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyOresAndShiniesRecipe(
  string Name,
  double Density,
  double MinimumYFactor,
  double MaximumYFactor,
  int MinimumStrength,
  int MaximumStrengthExclusive,
  int MinimumSteps,
  int MaximumStepsExclusive,
  int NormalTileType,
  int DrunkTileType)
{
  public int CalculateInvocationCount(int width, int height)
  {
    if (width <= 0 || height <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    return checked((int)(width * (double)height * Density));
  }

  public int SelectTileType(bool isDrunkWorld, LegacyPassRandomState random)
  {
    ArgumentNullException.ThrowIfNull(random);
    return isDrunkWorld && random.Next(2) != 0 ? DrunkTileType : NormalTileType;
  }

  public LegacyOreRunnerRequest CreateInvocation(
    int width,
    int minimumY,
    int maximumYExclusive,
    bool isDrunkWorld,
    LegacyPassRandomState random)
  {
    ArgumentNullException.ThrowIfNull(random);
    if (width <= 0 || minimumY < 0 || maximumYExclusive <= minimumY)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    int tileType = SelectTileType(isDrunkWorld, random);
    return new LegacyOreRunnerRequest(
      random.Next(width),
      random.Next(minimumY, maximumYExclusive),
      random.Next(MinimumStrength, MaximumStrengthExclusive),
      random.Next(MinimumSteps, MaximumStepsExclusive),
      TileType: tileType);
  }
}

public static class LegacyOresAndShiniesRecipeCatalog
{
  private static readonly IReadOnlyList<LegacyOresAndShiniesRecipe> RemixRecipes =
    Array.AsReadOnly<LegacyOresAndShiniesRecipe>(
    [
      new("copper-surface", 0.00006, 0, 1, 3, 6, 2, 6, 7, 166),
      new("copper-middle", 0.00008, 1, 2, 3, 7, 3, 7, 7, 166),
      new("copper-deep", 0.0002, 2, 3, 4, 9, 4, 8, 7, 166),
      new("iron-surface", 0.00003, 0, 1, 3, 7, 2, 5, 6, 167),
      new("iron-middle", 0.00008, 1, 2, 3, 6, 3, 6, 6, 167),
      new("iron-deep", 0.0002, 2, 3, 4, 9, 4, 8, 6, 167),
      new("silver-deep", 0.000026, 2, 3, 3, 6, 3, 6, 9, 168),
      new("silver-shallow", 0.00015, 1, 2, 4, 9, 4, 8, 9, 168),
      new("gold-shallow", 0.00012, 1, 2, 4, 8, 4, 8, 8, 169)
    ]);

  private static readonly IReadOnlyList<LegacyOresAndShiniesRecipe> NonRemixRecipes =
    Array.AsReadOnly<LegacyOresAndShiniesRecipe>(
    [
      new("copper-surface", 0.00006, 0, 1, 3, 6, 2, 6, 7, 166),
      new("copper-middle", 0.00008, 1, 2, 3, 7, 3, 7, 7, 166),
      new("copper-deep", 0.0002, 2, 3, 4, 9, 4, 8, 7, 166),
      new("iron-surface", 0.00003, 0, 1, 3, 7, 2, 5, 6, 167),
      new("iron-middle", 0.00008, 1, 2, 3, 6, 3, 6, 6, 167),
      new("iron-deep", 0.0002, 2, 3, 4, 9, 4, 8, 6, 167),
      new("silver-middle", 0.000026, 1, 2, 3, 6, 3, 6, 9, 168),
      new("silver-deep", 0.00015, 2, 3, 4, 9, 4, 8, 9, 168),
      new("gold-deep", 0.00012, 2, 3, 4, 8, 4, 8, 8, 169),
      new("silver-surface", 0.00017, 0, 1, 4, 9, 4, 8, 9, 168),
      new("gold-surface", 0.00012, 0, 1, 4, 8, 4, 8, 8, 169),
      new("evil-crimson-deep", 0.0000225, 2, 3, 3, 6, 4, 8, 204, 204),
      new("evil-corruption-deep", 0.0000225, 2, 3, 3, 6, 4, 8, 22, 22)
    ]);

  public static IReadOnlyList<LegacyOresAndShiniesRecipe> CreateRemixRecipes()
  {
    return RemixRecipes;
  }

  public static IReadOnlyList<LegacyOresAndShiniesRecipe> CreateNonRemixRecipes()
  {
    return NonRemixRecipes;
  }
}
