using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyTileRunnerSnapshotBatchExecutionResult(
  LegacyTileRunnerPassCommandBatch Commands,
  IReadOnlyList<LegacyTileRunnerInvocationProvenance> Provenance);

public static class LegacyTileRunnerSnapshotBatchExecution
{
  public static LegacyTileRunnerSnapshotBatchExecutionResult Execute(
    WorldGridSnapshot snapshot,
    string passName,
    IReadOnlyCollection<LegacyTileRunnerPassInvocation> invocations,
    LegacyTileRunnerSnapshotExecutionContext context,
    long startingSequence)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentException.ThrowIfNullOrEmpty(passName);
    ArgumentNullException.ThrowIfNull(invocations);
    ArgumentNullException.ThrowIfNull(context);
    if (startingSequence < 0 || startingSequence >= long.MaxValue - 2)
    {
      throw new ArgumentOutOfRangeException(nameof(startingSequence));
    }

    List<LegacyTileRunnerInvocationProvenance> provenance = new();
    List<LegacyTileRunnerCommandBatch> batches = new();
    long nextSequence = startingSequence;
    int invocationIndex = 0;
    foreach (LegacyTileRunnerPassInvocation invocation in invocations)
    {
      if (invocation.Recipe.PassName != passName)
      {
        throw new InvalidOperationException(
          "TileRunner snapshot batch contained an invocation from another pass.");
      }

      LegacyTileRunnerInvocationProvenance entry =
        LegacyTileRunnerSnapshotExecution.Execute(
          snapshot,
          invocation,
          context,
          nextSequence,
          nextSequence + 1,
          invocationIndex);
      provenance.Add(entry);
      batches.Add(entry.Commands);
      nextSequence += 2;
      invocationIndex++;
    }

    LegacyTileRunnerPassCommandBatch commandBatch =
      LegacyTileRunnerPassCommandBatchFactory.Create(
        passName,
        batches,
        startingSequence);
    return new LegacyTileRunnerSnapshotBatchExecutionResult(
      commandBatch,
      provenance.AsReadOnly());
  }
}
