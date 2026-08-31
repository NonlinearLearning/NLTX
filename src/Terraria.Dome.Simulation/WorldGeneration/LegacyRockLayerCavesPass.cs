using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyRockLayerCavesPass
{
  public const int SourceLine = 12605;

  private static readonly LegacyTileRunnerPassInput _recipe = new(
    "RockLayerCaves",
    "rock-layer",
    -1,
    false,
    6,
    20,
    50,
    300,
    "rockLayerHigh..maxTilesY",
    true,
    true,
    true);

  public static int SourceAnchorLine => SourceLine;

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
    if (isSkyblockWorld)
    {
      return;
    }

    if (!double.IsFinite(profile.RockLayerHigh) ||
        profile.RockLayerHigh > snapshot.Metadata.Height)
    {
      return;
    }

    profile.Validate(snapshot.Metadata);
    LegacyRockLayerCavesPassDefinition definition =
      LegacyRockLayerCavesPassDefinitionFactory.CreateDefault();
    definition.Validate();
    if (state.Stage < WorldGenerationStage.Cave &&
        !state.TryAdvance(WorldGenerationStage.Cave))
    {
      throw new InvalidOperationException("RockLayerCaves could not enter the cave stage.");
    }

    int rockLayerHigh = checked((int)profile.RockLayerHigh);
    if (rockLayerHigh >= snapshot.Metadata.Height)
    {
      return;
    }

    int worldSurfaceY = checked((int)profile.WorldSurface);
    int rockLayerY = checked((int)profile.RockLayer);
    int invocationCount = definition.CalculateInvocationCount(
      snapshot.Metadata.Width,
      snapshot.Metadata.Height,
      isRemixWorld);
    Dictionary<(int X, int Y), WorldTile> projectedTiles = new();
    for (int index = 0; index < invocationCount; index++)
    {
      LegacyTileRunnerPassInvocation invocation = CreateInvocation(
        snapshot,
        profile,
        definition,
        random,
        isRemixWorld);
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

  private static LegacyTileRunnerPassInvocation CreateInvocation(
    WorldGridSnapshot snapshot,
    LegacyTerrainRuntimeProfile profile,
    LegacyRockLayerCavesPassDefinition definition,
    LegacyPassRandomState random,
    bool isRemixWorld)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(profile);
    ArgumentNullException.ThrowIfNull(definition);
    ArgumentNullException.ThrowIfNull(random);
    int rockLayerHigh = checked((int)profile.RockLayerHigh);
    int tileType = definition.SelectTileType(random);
    int strength = definition.ScaleStrength(
      random.Next(definition.MinimumStrength, definition.MaximumStrengthExclusive),
      isRemixWorld);
    int steps = definition.ScaleSteps(
      random.Next(definition.MinimumSteps, definition.MaximumStepsExclusive),
      isRemixWorld);
    int x = random.Next(0, snapshot.Metadata.Width);
    int y = random.Next(rockLayerHigh, snapshot.Metadata.Height);
    LegacyTileRunnerRequest request = new(
      x,
      y,
      strength,
      steps,
      tileType,
      addTile: false,
      speedX: 0.0,
      speedY: 0.0,
      noYChange: false,
      overwrite: true,
      ignoreTileType: -1);
    return new LegacyTileRunnerPassInvocation(
      _recipe with { TileType = tileType },
      request,
      x,
      y,
      strength,
      steps,
      RandomDrawCount: 5);
  }
}
