using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyRocksInDirtPass
{
  public const int SourceLine = 12234;

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    LegacyTerrainRuntimeProfile profile,
    LegacyPassRandomState random,
    bool isSkyblockWorld,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(profile);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    profile.Validate(snapshot.Metadata);
    if (isSkyblockWorld)
    {
      return;
    }

    if (state.Stage < WorldGenerationStage.Cave &&
        !state.TryAdvance(WorldGenerationStage.Cave))
    {
      throw new InvalidOperationException("RocksInDirt could not enter the cave stage.");
    }

    int worldSurfaceLow = ProjectRange(profile.WorldSurfaceLow, snapshot.Metadata.Height);
    int worldSurfaceHigh = ProjectRange(profile.WorldSurfaceHigh, snapshot.Metadata.Height);
    int rockLayerHigh = ProjectRange(profile.RockLayerHigh, snapshot.Metadata.Height);
    int worldSurfaceY = LegacyMainWorldSurfacePolicy.Resolve(profile, snapshot.Metadata);
    int rockLayerY = ProjectRange(profile.RockLayer, snapshot.Metadata.Height);
    Dictionary<(int X, int Y), WorldTile> projectedTiles = new();
    IReadOnlyList<LegacyTileRunnerPassInput> recipes =
      LegacyRocksInDirtPassDefinition.CreateDefaultRecipes();
    IReadOnlyList<LegacyTileRunnerPassLoopDefinition> loops =
      LegacyRocksInDirtPassDefinition.CreateDefaultLoops();
    for (int recipeIndex = 0; recipeIndex < recipes.Count; recipeIndex++)
    {
      LegacyTileRunnerPassInput recipe = recipes[recipeIndex];
      LegacyTileRunnerPassLoopDefinition loop = loops[recipeIndex];
      int invocationCount = loop.CalculateInvocationCount(
        snapshot.Metadata.Width,
        snapshot.Metadata.Height,
        remixWorld: false);
      for (int invocationIndex = 0; invocationIndex < invocationCount; invocationIndex++)
      {
        (int minimumY, int maximumYExclusive) = recipeIndex switch
        {
          0 => (0, checked(worldSurfaceLow + 1)),
          1 => (worldSurfaceLow, checked(worldSurfaceHigh + 1)),
          2 => (worldSurfaceHigh, checked(rockLayerHigh + 1)),
          _ => throw new InvalidOperationException("RocksInDirt recipe index was invalid.")
        };
        LegacyTileRunnerPassInvocation invocation = CreateInvocation(
          snapshot,
          recipe,
          random,
          minimumY,
          maximumYExclusive,
          rerollInactiveOffset: recipeIndex == 1,
          projectedTiles);
        LegacyTileRunnerTraversal.AppendCommands(
          snapshot,
          invocation,
          random,
          worldSurfaceY,
          rockLayerY,
          ref state,
          commands,
          projectedTiles: projectedTiles);
      }
    }
  }

  private static LegacyTileRunnerPassInvocation CreateInvocation(
    WorldGridSnapshot snapshot,
    LegacyTileRunnerPassInput recipe,
    LegacyPassRandomState random,
    int minimumY,
    int maximumYExclusive,
    bool rerollInactiveOffset,
    IReadOnlyDictionary<(int X, int Y), WorldTile> projectedTiles)
  {
    int xDraw = random.Next(0, snapshot.Metadata.Width);
    int yDraw = random.Next(minimumY, maximumYExclusive);
    int randomDrawCount = 2;
    int offsetY = Math.Max(0, yDraw - 10);
    WorldTile offsetTile = projectedTiles.TryGetValue((xDraw, offsetY), out WorldTile projectedTile)
      ? projectedTile
      : snapshot.GetTile(xDraw, offsetY);
    if (rerollInactiveOffset && yDraw >= 10 && !offsetTile.IsActive)
    {
      yDraw = random.Next(minimumY, maximumYExclusive);
      randomDrawCount++;
    }

    int strengthDraw = random.Next(recipe.MinimumStrength, recipe.MaximumStrengthExclusive);
    int stepsDraw = random.Next(recipe.MinimumSteps, recipe.MaximumStepsExclusive);
    randomDrawCount += 2;
    LegacyTileRunnerRequest request = new(
      xDraw,
      yDraw,
      strengthDraw,
      stepsDraw,
      recipe.TileType,
      recipe.AddTile,
      speedX: 0.0,
      speedY: 0.0,
      noYChange: false,
      overwrite: true,
      ignoreTileType: -1);
    return new LegacyTileRunnerPassInvocation(
      recipe,
      request,
      xDraw,
      yDraw,
      strengthDraw,
      stepsDraw,
      randomDrawCount);
  }

  private static int ProjectRange(double value, int height)
  {
    int projected = checked((int)value);
    if (projected < 0 || projected >= height)
    {
      throw new ArgumentOutOfRangeException(nameof(value));
    }

    return projected;
  }
}
