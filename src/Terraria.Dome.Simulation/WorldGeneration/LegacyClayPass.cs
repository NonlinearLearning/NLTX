using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyClayPass
{
  public const int SourceLine = 12383;

  private const int CleanupColumnMargin = 5;
  private const int CleanupRowCount = 5;
  private const int CleanupPriority = 1;

  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    LegacyTerrainRuntimeProfile profile,
    LegacyPassRandomState random,
    bool isRemixWorld,
    bool isSkyblockWorld,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(profile);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(commands);
    profile.Validate(snapshot.Metadata);
    LegacyClayPassDefinition.Validate();
    if (isSkyblockWorld)
    {
      return;
    }

    if (state.Stage < WorldGenerationStage.Cave &&
        !state.TryAdvance(WorldGenerationStage.Cave))
    {
      throw new InvalidOperationException("Clay could not enter the cave stage.");
    }

    IReadOnlyList<LegacyTileRunnerPassInput> recipes =
      LegacyClayPassDefinition.CreateDefaultRecipes();
    int worldSurfaceLow = ConvertProfileCoordinate(
      profile.WorldSurfaceLow,
      nameof(profile.WorldSurfaceLow));
    int worldSurfaceHigh = ConvertProfileCoordinate(
      profile.WorldSurfaceHigh,
      nameof(profile.WorldSurfaceHigh));
    int rockLayer = ConvertProfileCoordinate(profile.RockLayer, nameof(profile.RockLayer));
    int rockLayerHigh = ConvertProfileCoordinate(
      profile.RockLayerHigh,
      nameof(profile.RockLayerHigh));
    int worldSurfaceY = ConvertProfileCoordinate(
      profile.WorldSurface,
      nameof(profile.WorldSurface));
    Dictionary<(int X, int Y), WorldTile> projectedTiles = new();

    AppendRecipeCommands(
      snapshot,
      recipes[0],
      LegacyClayPassDefinition.SurfaceLowDensity,
      minimumYInclusive: 0,
      maximumYExclusive: worldSurfaceLow,
      random,
      worldSurfaceY,
      rockLayer,
      ref state,
      commands,
      projectedTiles);

    if (isRemixWorld)
    {
      AppendRecipeCommands(
        snapshot,
        recipes[1],
        LegacyClayPassDefinition.RemixDensity,
        minimumYInclusive: checked(rockLayer - 25),
        maximumYExclusive: checked(snapshot.Metadata.Height - 350),
        random,
        worldSurfaceY,
        rockLayer,
        ref state,
        commands,
        projectedTiles);
    }
    else
    {
      AppendRecipeCommands(
        snapshot,
        recipes[2],
        LegacyClayPassDefinition.SurfaceHighDensity,
        minimumYInclusive: worldSurfaceLow,
        maximumYExclusive: checked(worldSurfaceHigh + 1),
        random,
        worldSurfaceY,
        rockLayer,
        ref state,
        commands,
        projectedTiles);
      AppendRecipeCommands(
        snapshot,
        recipes[3],
        LegacyClayPassDefinition.RockHighDensity,
        minimumYInclusive: worldSurfaceHigh,
        maximumYExclusive: checked(rockLayerHigh + 1),
        random,
        worldSurfaceY,
        rockLayer,
        ref state,
        commands,
        projectedTiles);
    }

    AppendCleanupCommands(
      snapshot,
      LegacyMainWorldSurfacePolicy.Resolve(profile, snapshot.Metadata),
      ref state,
      commands,
      projectedTiles);
  }

  private static void AppendRecipeCommands(
    WorldGridSnapshot snapshot,
    LegacyTileRunnerPassInput recipe,
    double density,
    int minimumYInclusive,
    int maximumYExclusive,
    LegacyPassRandomState random,
    int worldSurfaceY,
    int rockLayerY,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands,
    Dictionary<(int X, int Y), WorldTile> projectedTiles)
  {
    ValidateVerticalRange(
      minimumYInclusive,
      maximumYExclusive,
      snapshot.Metadata.Height,
      recipe.RecipeName);
    int invocationCount = LegacyClayPassDefinition.CalculateInvocationCount(
      snapshot.Metadata.Width,
      snapshot.Metadata.Height,
      density);
    for (int index = 0; index < invocationCount; index++)
    {
      LegacyTileRunnerPassInvocation invocation =
        LegacyTileRunnerPassInvocationFactory.Create(
          recipe,
          random,
          minimumXInclusive: 0,
          maximumXExclusive: snapshot.Metadata.Width,
          minimumYInclusive,
          maximumYExclusive);
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

  private static void AppendCleanupCommands(
    WorldGridSnapshot snapshot,
    double worldSurface,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands,
    Dictionary<(int X, int Y), WorldTile> projectedTiles)
  {
    for (int x = CleanupColumnMargin;
         x < snapshot.Metadata.Width - CleanupColumnMargin;
         x++)
    {
      for (int y = 1; (double)y < worldSurface - 1.0; y++)
      {
        WorldTile tile = GetProjectedTile(snapshot, projectedTiles, x, y);
        if (!tile.IsActive)
        {
          continue;
        }

        for (int offset = 0; offset < CleanupRowCount; offset++)
        {
          int cleanupY = y + offset;
          if (cleanupY >= snapshot.Metadata.Height)
          {
            break;
          }

          (int X, int Y) coordinates = (x, cleanupY);
          WorldTile cleanupTile = GetProjectedTile(
            snapshot,
            projectedTiles,
            coordinates.X,
            coordinates.Y);
          if (cleanupTile.Type != LegacyClayPassDefinition.TargetTileType)
          {
            continue;
          }

          TileChangeCommand command = new(
            state.ReserveSequence(),
            coordinates.X,
            coordinates.Y,
            TileChangeKind.UpdateTileType,
            0,
            Priority: CleanupPriority,
            Source: "worldgen.cave.Clay.cleanup",
            IsActive: cleanupTile.IsActive);
          commands.Add(command);
          projectedTiles[coordinates] = TileMutationProjection.Apply(cleanupTile, command);
        }

        break;
      }
    }
  }

  private static WorldTile GetProjectedTile(
    WorldGridSnapshot snapshot,
    IReadOnlyDictionary<(int X, int Y), WorldTile> projectedTiles,
    int x,
    int y)
  {
    return projectedTiles.TryGetValue((x, y), out WorldTile projectedTile)
      ? projectedTile
      : snapshot.GetTile(x, y);
  }

  private static int ConvertProfileCoordinate(double value, string parameterName)
  {
    if (!double.IsFinite(value))
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }

    return checked((int)value);
  }

  private static void ValidateVerticalRange(
    int minimumYInclusive,
    int maximumYExclusive,
    int height,
    string recipeName)
  {
    if (minimumYInclusive < 0 || maximumYExclusive <= minimumYInclusive ||
        maximumYExclusive > height)
    {
      throw new ArgumentOutOfRangeException(
        nameof(recipeName),
        $"Clay recipe {recipeName} had an invalid vertical range.");
    }
  }
}
