using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyExtraLiquidPassExecution
{
  private static readonly IReadOnlyList<LiquidDefinition> LiquidDefinitions =
    Array.AsReadOnly(
    [
      new LiquidDefinition("water", 0, byte.MaxValue),
      new LiquidDefinition("lava", 1, byte.MaxValue),
      new LiquidDefinition("honey", 2, byte.MaxValue)
    ]);

  public static LegacyExtraLiquidExecutionResult TryExecuteBubbleBlocks(
    WorldGrid world,
    WorldMetadata metadata,
    int worldSurfaceY,
    int underworldLayerY,
    bool isRemixWorld,
    bool isSkyblockWorld,
    LegacyPassRandomState random,
    ref WorldGenerationStateComponent state)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(metadata);
    ArgumentNullException.ThrowIfNull(random);
    if (world.Width != metadata.Width || world.Height != metadata.Height)
    {
      throw new ArgumentException("World dimensions must match ExtraLiquid metadata.", nameof(metadata));
    }

    WorldGenerationStateComponent originalState = state;
    List<TileChangeCommand> tileCommands = new();
    List<LiquidChangeCommand> liquidCommands = new();
    IReadOnlyList<LegacyExtraLiquidBubbleSquare> bubbles =
      LegacyExtraLiquidAddBubbleBlocks.AppendCommands(
        world.CreateSnapshot(metadata),
        worldSurfaceY,
        underworldLayerY,
        isRemixWorld,
        isSkyblockWorld,
        random,
        ref state,
        tileCommands,
        liquidCommands);
    LegacyTileRunnerPassCommandBatch batch = new(
      "ExtraLiquid.AddBubbleBlocks",
      tileCommands.AsReadOnly(),
      liquidCommands.AsReadOnly(),
      state.NextSequence);
    if (!LegacyTileRunnerCommandCommitBoundary.TryCommit(
          world,
          batch,
          LiquidDefinitions,
          out LegacyTileRunnerCommandCommitResult commitResult))
    {
      state = originalState;
      return LegacyExtraLiquidExecutionResult.Failed(
        state.NextSequence,
        commitResult.FailureReason ?? "ExtraLiquid bubble command commit failed.");
    }

    return new LegacyExtraLiquidExecutionResult(
      true,
      commitResult.AppliedTileCount,
      commitResult.AppliedLiquidCount,
      state.NextSequence,
      bubbles,
      null);
  }
}
