using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public enum LegacySurfaceCavesVerticalFamily
{
  Narrow,
  Medium,
  Deep,
  Horizontal
}

public sealed record LegacySurfaceCavesVerticalRecipe(
  LegacySurfaceCavesVerticalFamily Family,
  double WidthDensity,
  int MinimumStrength,
  int MaximumStrengthExclusive,
  int MinimumSteps,
  int MaximumStepsExclusive,
  double SpeedY,
  bool NoYChange,
  int TileType,
  bool AddTile);

public static class LegacySurfaceCavesVerticalPass
{
  private const int BeachPadding = 20;
  private static readonly IReadOnlyList<LegacySurfaceCavesVerticalRecipe> DefaultRecipes =
    Array.AsReadOnly<LegacySurfaceCavesVerticalRecipe>(
    [
      new LegacySurfaceCavesVerticalRecipe(
        LegacySurfaceCavesVerticalFamily.Narrow,
        0.002,
        3,
        6,
        5,
        50,
        1.0,
        false,
        -1,
        false),
      new LegacySurfaceCavesVerticalRecipe(
        LegacySurfaceCavesVerticalFamily.Medium,
        0.0007,
        10,
        15,
        50,
        130,
        2.0,
        false,
        -1,
        false),
      new LegacySurfaceCavesVerticalRecipe(
        LegacySurfaceCavesVerticalFamily.Deep,
        0.0003,
        12,
        25,
        150,
        500,
        4.0,
        false,
        -1,
        false),
      new LegacySurfaceCavesVerticalRecipe(
        LegacySurfaceCavesVerticalFamily.Horizontal,
        0.0004,
        7,
        12,
        150,
        250,
        1.0,
        true,
        -1,
        false)
    ]);

  public static IReadOnlyList<LegacySurfaceCavesVerticalRecipe> CreateDefaultRecipes()
  {
    return DefaultRecipes;
  }

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    LegacyTerrainRuntimeProfile profile,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands,
    bool isRemixWorld = false,
    IDictionary<(int X, int Y), WorldTile>? projectedTiles = null)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(profile);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    profile.Validate(snapshot.Metadata);
    LegacySurfaceCavesPassDefinition definition =
      LegacySurfaceCavesPassDefinitionFactory.CreateDefault();
    definition.Validate();
    projectedTiles ??= new Dictionary<(int X, int Y), WorldTile>();
    foreach (LegacySurfaceCavesVerticalRecipe recipe in DefaultRecipes)
    {
      int count = CalculateInvocationCount(
        recipe.Family,
        snapshot.Metadata.Width,
        isRemixWorld);
      if (count == 0 || !HasAcceptedStartX(snapshot.Metadata.Width, profile, recipe.Family))
      {
        continue;
      }

      for (int index = 0; index < count; index++)
      {
        int startX = SelectStartX(snapshot.Metadata.Width, profile, recipe.Family, random);
        int startY = FindSurfaceY(
          snapshot,
          startX,
          (int)profile.WorldSurfaceHigh,
          projectedTiles);
        if (startY < 0)
        {
          continue;
        }

        AppendRunner(
          snapshot,
          profile,
          recipe,
          startX,
          startY,
          random,
          ref state,
          commands,
          projectedTiles);
        if (recipe.Family == LegacySurfaceCavesVerticalFamily.Deep)
        {
          AppendRunner(
            snapshot,
            profile,
            recipe with
            {
              MinimumStrength = 8,
              MaximumStrengthExclusive = 17,
              MinimumSteps = 60,
              MaximumStepsExclusive = 200,
              SpeedY = 2.0
            },
            startX,
            startY,
            random,
            ref state,
            commands,
            projectedTiles);
          AppendRunner(
            snapshot,
            profile,
            recipe with
            {
              MinimumStrength = 5,
              MaximumStrengthExclusive = 13,
              MinimumSteps = 40,
              MaximumStepsExclusive = 170,
              SpeedY = 2.0
            },
            startX,
            startY,
            random,
            ref state,
            commands,
            projectedTiles);
        }
      }
    }
  }

  public static int CalculateInvocationCount(
    LegacySurfaceCavesVerticalFamily family,
    int width,
    bool isRemixWorld = false)
  {
    LegacySurfaceCavesPassDefinition definition =
      LegacySurfaceCavesPassDefinitionFactory.CreateDefault();
    definition.Validate();
    return definition.CalculateInvocationCount(family, width, isRemixWorld);
  }

  public static double DrawSpeedX(
    LegacySurfaceCavesVerticalRecipe recipe,
    LegacyPassRandomState random)
  {
    ArgumentNullException.ThrowIfNull(recipe);
    ArgumentNullException.ThrowIfNull(random);
    return recipe.NoYChange ? 0.0 : random.Next(-10, 11) * 0.1;
  }

  private static void AppendRunner(
    WorldGridSnapshot snapshot,
    LegacyTerrainRuntimeProfile profile,
    LegacySurfaceCavesVerticalRecipe recipe,
    int startX,
    int startY,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands,
    IDictionary<(int X, int Y), WorldTile> projectedTiles)
  {
    LegacyTileRunnerPassInput passInput = new(
      "SurfaceCaves",
      "vertical-" + recipe.Family.ToString().ToLowerInvariant(),
      recipe.TileType,
      recipe.AddTile,
      recipe.MinimumStrength,
      recipe.MaximumStrengthExclusive,
      recipe.MinimumSteps,
      recipe.MaximumStepsExclusive,
      "surface-active",
      false,
      false,
      true);
    LegacyTileRunnerRequest request = new(
      startX,
      startY,
      random.Next(recipe.MinimumStrength, recipe.MaximumStrengthExclusive),
      random.Next(recipe.MinimumSteps, recipe.MaximumStepsExclusive),
      recipe.TileType,
      recipe.AddTile,
      DrawSpeedX(recipe, random),
      recipe.SpeedY,
      recipe.NoYChange,
      true,
      -1);
    LegacyTileRunnerPassInvocation invocation = new(passInput, request, startX, startY,
      (int)request.Strength, request.Steps, 4);
    LegacyTileRunnerTraversal.AppendCommands(
      snapshot,
      invocation,
      random,
      (int)profile.WorldSurface,
      (int)profile.RockLayer,
      ref state,
      commands,
      projectedTiles: projectedTiles);
  }

  private static int FindSurfaceY(
    WorldGridSnapshot snapshot,
    int x,
    int maximumYExclusive,
    IDictionary<(int X, int Y), WorldTile> projectedTiles)
  {
    ArgumentNullException.ThrowIfNull(projectedTiles);
    for (int y = 0; y < Math.Min(maximumYExclusive, snapshot.Metadata.Height); y++)
    {
      WorldTile tile = projectedTiles.TryGetValue((x, y), out WorldTile projectedTile)
        ? projectedTile
        : snapshot.GetTile(x, y);
      if (tile.IsActive)
      {
        return y;
      }
    }

    return -1;
  }

  private static int SelectStartX(
    int width,
    LegacyTerrainRuntimeProfile profile,
    LegacySurfaceCavesVerticalFamily family,
    LegacyPassRandomState random)
  {
    while (true)
    {
      int x = random.Next(width);
      double centerMinimum = family is LegacySurfaceCavesVerticalFamily.Narrow ? 0.45 :
        family is LegacySurfaceCavesVerticalFamily.Medium ? 0.43 : 0.4;
      double centerMaximum = family is LegacySurfaceCavesVerticalFamily.Narrow ? 0.55 :
        family is LegacySurfaceCavesVerticalFamily.Medium ? 0.57 : 0.6;
      if ((x <= width * centerMinimum || x >= width * centerMaximum) &&
          x >= profile.LeftBeachEnd + BeachPadding &&
          x <= profile.RightBeachStart - BeachPadding)
      {
        return x;
      }
    }
  }

  private static bool HasAcceptedStartX(
    int width,
    LegacyTerrainRuntimeProfile profile,
    LegacySurfaceCavesVerticalFamily family)
  {
    for (int x = 0; x < width; x++)
    {
      double centerMinimum = family is LegacySurfaceCavesVerticalFamily.Narrow ? 0.45 :
        family is LegacySurfaceCavesVerticalFamily.Medium ? 0.43 : 0.4;
      double centerMaximum = family is LegacySurfaceCavesVerticalFamily.Narrow ? 0.55 :
        family is LegacySurfaceCavesVerticalFamily.Medium ? 0.57 : 0.6;
      if ((x <= width * centerMinimum || x >= width * centerMaximum) &&
          x >= profile.LeftBeachEnd + BeachPadding &&
          x <= profile.RightBeachStart - BeachPadding)
      {
        return true;
      }
    }

    return false;
  }
}
