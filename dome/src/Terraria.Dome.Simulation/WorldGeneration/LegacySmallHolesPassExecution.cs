using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacySmallHolesPassExecution
{
  private static readonly LegacyTileRunnerPassInput Recipe = new(
    "SmallHoles",
    "small-holes",
    -1,
    false,
    2,
    5,
    2,
    20,
    "worldSurfaceHigh..maxTilesY",
    true,
    true,
    true);

  public static LegacyTileRunnerSnapshotBatchExecutionResult Execute(
    WorldGridSnapshot snapshot,
    LegacySmallHolesPassDefinition definition,
    LegacyPassRandomState random,
    LegacyTileRunnerSnapshotExecutionContext context,
    long startingSequence,
    int iterationCount)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(definition);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(context);
    if (iterationCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(iterationCount));
    }

    List<LegacyTileRunnerPassInvocation> invocations = new(iterationCount * 2);
    for (int index = 0; index < iterationCount; index++)
    {
      LegacySmallHolesInvocationPair pair = LegacySmallHolesInvocationFactory.Create(
        definition,
        random,
        snapshot.Metadata.Width,
        snapshot.Metadata.Height,
        Math.Clamp((int)Math.Ceiling(context.WorldSurface), 0, snapshot.Metadata.Height - 1));
      invocations.Add(CreateInvocation(pair.First));
      invocations.Add(CreateInvocation(pair.Second));
    }

    return LegacyTileRunnerSnapshotBatchExecution.Execute(
      snapshot,
      Recipe.PassName,
      invocations,
      context,
      startingSequence);
  }

  public static void AppendEnvelopeCommands(
    WorldGridSnapshot snapshot,
    LegacySmallHolesPassDefinition definition,
    LegacyPassRandomState random,
    LegacyTileRunnerLiquidContext liquidContext,
    int worldSurfaceY,
    int rockLayerY,
    int iterationCount,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> tileCommands,
    List<LiquidChangeCommand> liquidCommands)
  {
    AppendEnvelopeCommands(
      snapshot,
      definition,
      random,
      liquidContext,
      worldSurfaceY,
      rockLayerY,
      iterationCount,
      isSkyblockWorld: false,
      ref state,
      tileCommands,
      liquidCommands);
  }

  public static void AppendEnvelopeCommands(
    WorldGridSnapshot snapshot,
    LegacySmallHolesPassDefinition definition,
    LegacyPassRandomState random,
    LegacyTileRunnerLiquidContext liquidContext,
    int worldSurfaceY,
    int rockLayerY,
    int iterationCount,
    bool isSkyblockWorld,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> tileCommands,
    List<LiquidChangeCommand> liquidCommands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(definition);
    ArgumentNullException.ThrowIfNull(random);
    ArgumentNullException.ThrowIfNull(liquidContext);
    ArgumentNullException.ThrowIfNull(tileCommands);
    ArgumentNullException.ThrowIfNull(liquidCommands);
    if (iterationCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(iterationCount));
    }

    if (isSkyblockWorld)
    {
      return;
    }

    liquidContext.Validate();
    for (int index = 0; index < iterationCount; index++)
    {
      LegacySmallHolesInvocationPair pair = LegacySmallHolesInvocationFactory.Create(
        definition,
        random,
        snapshot.Metadata.Width,
        snapshot.Metadata.Height,
        worldSurfaceY);
      AppendEnvelopeCommands(
        snapshot,
        pair.First,
        random,
        liquidContext,
        worldSurfaceY,
        rockLayerY,
        ref state,
        tileCommands,
        liquidCommands);
      AppendEnvelopeCommands(
        snapshot,
        pair.Second,
        random,
        liquidContext,
        worldSurfaceY,
        rockLayerY,
        ref state,
        tileCommands,
        liquidCommands);
    }
  }

  private static LegacyTileRunnerPassInvocation CreateInvocation(LegacyTileRunnerRequest request)
  {
    return new LegacyTileRunnerPassInvocation(
      Recipe with { TileType = request.TileType },
      request,
      request.X,
      request.Y,
      (int)request.Strength,
      request.Steps,
      RandomDrawCount: 4);
  }

  private static void AppendEnvelopeCommands(
    WorldGridSnapshot snapshot,
    LegacyTileRunnerRequest request,
    LegacyPassRandomState random,
    LegacyTileRunnerLiquidContext liquidContext,
    int worldSurfaceY,
    int rockLayerY,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> tileCommands,
    List<LiquidChangeCommand> liquidCommands)
  {
    LegacyTileRunnerPassInvocation invocation = CreateInvocation(request);
    LegacyTileRunnerTraversal.AppendCommands(
      snapshot,
      invocation,
      random,
      worldSurfaceY,
      rockLayerY,
      ref state,
      tileCommands,
      liquidContext,
      liquidCommands);
  }
}
