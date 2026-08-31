using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyOresAndShiniesPass
{
  public static (int MinimumY, int MaximumYExclusive) GetProfileYRange(
    LegacyOresAndShiniesRecipe recipe,
    LegacyTerrainRuntimeProfile profile,
    int height,
    bool isRemixWorld = false)
  {
    ArgumentNullException.ThrowIfNull(recipe);
    ArgumentNullException.ThrowIfNull(profile);
    if (height <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(height));
    }

    return ResolveProfileYRange(recipe, profile, height, isRemixWorld);
  }

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    LegacyTerrainRuntimeProfile profile,
    LegacyPassRandomState random,
    bool isRemixWorld,
    bool isDrunkWorld,
    bool isSkyblockWorld,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(profile);
    profile.Validate(snapshot.Metadata);
    AppendCommandsCore(
      snapshot,
      profile,
      random,
      isRemixWorld,
      isDrunkWorld,
      isSkyblockWorld,
      ref state,
      commands);
  }

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    LegacyPassRandomState random,
    bool isRemixWorld,
    bool isDrunkWorld,
    bool isSkyblockWorld,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    AppendCommandsCore(
      snapshot,
      profile: null,
      random,
      isRemixWorld,
      isDrunkWorld,
      isSkyblockWorld,
      ref state,
      commands);
  }

  private static void AppendCommandsCore(
    WorldGridSnapshot snapshot,
    LegacyTerrainRuntimeProfile? profile,
    LegacyPassRandomState random,
    bool isRemixWorld,
    bool isDrunkWorld,
    bool isSkyblockWorld,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    if (isSkyblockWorld)
    {
      return;
    }

    IReadOnlyList<LegacyOresAndShiniesRecipe> recipes = isRemixWorld
      ? LegacyOresAndShiniesRecipeCatalog.CreateRemixRecipes()
      : LegacyOresAndShiniesRecipeCatalog.CreateNonRemixRecipes();
    foreach (LegacyOresAndShiniesRecipe recipe in recipes)
    {
      int count = recipe.CalculateInvocationCount(snapshot.Metadata.Width, snapshot.Metadata.Height);
      (int minimumY, int maximumYExclusive) = profile is null
        ? GetGenericYRange(recipe, snapshot.Metadata.Height)
        : ResolveProfileYRange(recipe, profile, snapshot.Metadata.Height, isRemixWorld);
      for (int index = 0; index < count; index++)
      {
        LegacyOreRunnerRequest request = recipe.CreateInvocation(
          snapshot.Metadata.Width,
          minimumY,
          maximumYExclusive,
          isDrunkWorld,
          random);
        LegacyOreRunner.AppendCommands(snapshot, request, random, ref state, commands);
      }
    }
  }

  private static (int MinimumY, int MaximumYExclusive) GetGenericYRange(
    LegacyOresAndShiniesRecipe recipe,
    int height)
  {
    int minimumY = Math.Clamp((int)Math.Floor(recipe.MinimumYFactor * height), 0, height - 1);
    int maximumYExclusive = Math.Clamp(
      (int)Math.Ceiling(recipe.MaximumYFactor * height),
      minimumY + 1,
      height);
    return (minimumY, maximumYExclusive);
  }

  private static (int MinimumY, int MaximumYExclusive) ResolveProfileYRange(
    LegacyOresAndShiniesRecipe recipe,
    LegacyTerrainRuntimeProfile profile,
    int height,
    bool isRemixWorld)
  {
    int minimumY;
    int maximumYExclusive;
    if (!isRemixWorld &&
        recipe.Name is "silver-surface" or "gold-surface")
    {
      minimumY = 0;
      maximumYExclusive = (int)profile.WorldSurfaceLow;
    }
    else if (isRemixWorld && recipe.Name == "silver-deep")
    {
      minimumY = (int)profile.RockLayer - 100;
      maximumYExclusive = height - 250;
    }
    else if (recipe.MinimumYFactor < 1.0)
    {
      minimumY = (int)profile.WorldSurfaceLow;
      maximumYExclusive = (int)profile.WorldSurfaceHigh;
    }
    else if (recipe.MinimumYFactor < 2.0)
    {
      minimumY = (int)profile.WorldSurfaceHigh;
      maximumYExclusive = (int)profile.RockLayerHigh;
    }
    else
    {
      minimumY = (int)profile.RockLayerLow;
      maximumYExclusive = height;
    }

    return (Math.Clamp(minimumY, 0, height - 1), Math.Clamp(
      maximumYExclusive,
      Math.Clamp(minimumY, 0, height - 1) + 1,
      height));
  }
}
