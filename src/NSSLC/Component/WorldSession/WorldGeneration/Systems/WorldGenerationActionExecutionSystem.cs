using System;
using System.Collections.Generic;
using System.Threading;
using Terraria.WorldGeneration.Actions;
using Terraria.WorldGeneration.Adapters;

namespace Terraria.WorldGeneration.Systems;

public static class WorldGenerationActionExecutionSystem
{
  public static WorldGenerationActionExecutionResult Execute(
    long generationId,
    IReadOnlyList<WorldGenerationAction> actions,
    IWorldGenerationActionCommitPort commitPort,
    WorldGenerationActionFailurePolicy failurePolicy =
      WorldGenerationActionFailurePolicy.StopOnFailure,
    CancellationToken cancellationToken = default)
  {
    WorldGenerationActionExecutionBatch batch = Prepare(
      generationId,
      actions);
    return Commit(batch, commitPort, failurePolicy, cancellationToken);
  }

  public static WorldGenerationActionExecutionBatch Prepare(
    long generationId,
    IReadOnlyList<WorldGenerationAction> actions)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    ArgumentNullException.ThrowIfNull(actions);
    ValidateSequence(generationId, actions);
    return new WorldGenerationActionExecutionBatch(
      generationId,
      new List<WorldGenerationAction>(actions).AsReadOnly());
  }

  public static WorldGenerationActionExecutionResult Commit(
    WorldGenerationActionExecutionBatch batch,
    IWorldGenerationActionCommitPort commitPort,
    WorldGenerationActionFailurePolicy failurePolicy =
      WorldGenerationActionFailurePolicy.StopOnFailure,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(batch);
    ArgumentNullException.ThrowIfNull(commitPort);
    if (!Enum.IsDefined(failurePolicy))
    {
      throw new ArgumentOutOfRangeException(nameof(failurePolicy));
    }

    IReadOnlyList<WorldGenerationAction> actions = batch.Actions;
    List<WorldGenerationActionExecutionRecord> records =
      new(actions.Count);
    List<WorldGenerationAction> uncommittedActions = [];
    int? firstFailedIndex = null;
    bool stoppedOnFailure = false;
    for (int index = 0; index < actions.Count; index++)
    {
      if (cancellationToken.IsCancellationRequested)
      {
        AddUncommittedActions(actions, index, uncommittedActions);
        return new WorldGenerationActionExecutionResult(
          batch.GenerationId,
          records.AsReadOnly(),
          firstFailedIndex,
          stoppedOnFailure,
          uncommittedActions.AsReadOnly(),
          cancelled: true,
          cancellationReason: "CancellationRequested");
      }

      WorldGenerationAction action = actions[index];
      WorldGenerationActionCommitResult commitResult;
      try
      {
        commitResult = commitPort.Commit(in action);
      }
      catch (OperationCanceledException)
      {
        AddUncommittedActions(actions, index, uncommittedActions);
        return new WorldGenerationActionExecutionResult(
          batch.GenerationId,
          records.AsReadOnly(),
          firstFailedIndex,
          stoppedOnFailure,
          uncommittedActions.AsReadOnly(),
          cancelled: true,
          cancellationReason: "CommitPortCancelled");
      }
      records.Add(
        new WorldGenerationActionExecutionRecord(
          action.Sequence,
          action.Payload.Kind,
          commitResult.Accepted,
          commitResult.RejectionReason));

      if (!commitResult.Accepted)
      {
        firstFailedIndex ??= index;
        if (failurePolicy == WorldGenerationActionFailurePolicy.StopOnFailure)
        {
          stoppedOnFailure = true;
          AddUncommittedActions(actions, index + 1, uncommittedActions);

          break;
        }
      }
    }

    return new WorldGenerationActionExecutionResult(
      batch.GenerationId,
      records.AsReadOnly(),
      firstFailedIndex,
      stoppedOnFailure,
      uncommittedActions.AsReadOnly());
  }

  private static void AddUncommittedActions(
    IReadOnlyList<WorldGenerationAction> actions,
    int startIndex,
    List<WorldGenerationAction> destination)
  {
    for (int index = startIndex; index < actions.Count; index++)
    {
      destination.Add(actions[index]);
    }
  }

  private static void ValidateSequence(
    long generationId,
    IReadOnlyList<WorldGenerationAction> actions)
  {
    long previousSequence = -1;
    for (int index = 0; index < actions.Count; index++)
    {
      WorldGenerationAction action = actions[index];
      action.Validate();
      if (action.GenerationId != generationId)
      {
        throw new ArgumentException(
          "All actions must belong to the execution generation.",
          nameof(actions));
      }

      if (action.Sequence <= previousSequence)
      {
        throw new ArgumentException(
          "World-generation action sequences must be strictly increasing.",
          nameof(actions));
      }

      previousSequence = action.Sequence;
    }
  }
}
