using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyTileRunnerSnapshotCommitHandoff
{
  public static bool TryCommit(
    WorldGrid world,
    LegacyTileRunnerSnapshotBatchExecutionResult execution,
    IReadOnlyCollection<LiquidDefinition> liquidDefinitions,
    out LegacyTileRunnerCommandCommitResult result)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(execution);
    ArgumentNullException.ThrowIfNull(liquidDefinitions);
    int tileCount = 0;
    int liquidCount = 0;
    foreach (LegacyTileRunnerInvocationProvenance provenance in execution.Provenance)
    {
      if (provenance is null)
      {
        result = LegacyTileRunnerCommandCommitResult.Failed(
          "TileRunner snapshot handoff contained null provenance.");
        return false;
      }

      tileCount += provenance.Commands.TileCommand.HasValue ? 1 : 0;
      liquidCount += provenance.Commands.LiquidCommand.HasValue ? 1 : 0;
    }

    if (tileCount != execution.Commands.TileCommands.Count ||
        liquidCount != execution.Commands.LiquidCommands.Count)
    {
      result = LegacyTileRunnerCommandCommitResult.Failed(
        "TileRunner snapshot provenance did not match its command batch.");
      return false;
    }

    return LegacyTileRunnerCommandCommitBoundary.TryCommit(
      world,
      execution.Commands,
      liquidDefinitions,
      out result);
  }
}
