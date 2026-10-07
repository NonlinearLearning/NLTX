using System;
using System.Collections.Generic;
using Terraria.WorldGeneration.Actions;
using Terraria.WorldGeneration.Systems;

namespace Terraria.WorldGeneration.Adapters;

/// <summary>
/// Read-only value projection of an action execution result.
/// </summary>
public sealed class WorldGenerationActionResultProjection
{
  private WorldGenerationActionResultProjection(
    long? generationId,
    IReadOnlyList<WorldGenerationActionExecutionRecord> records,
    IReadOnlyList<WorldGenerationAction> uncommittedActions,
    int? firstFailedIndex,
    string? firstFailureReason,
    bool continuedAfterFailure,
    bool stoppedOnFailure,
    bool cancelled,
    string? cancellationReason,
    bool succeeded,
    bool completedAllActions,
    int committedCount)
  {
    GenerationId = generationId;
    Records = records;
    UncommittedActions = uncommittedActions;
    FirstFailedIndex = firstFailedIndex;
    FirstFailureReason = firstFailureReason;
    ContinuedAfterFailure = continuedAfterFailure;
    StoppedOnFailure = stoppedOnFailure;
    Cancelled = cancelled;
    CancellationReason = cancellationReason;
    Succeeded = succeeded;
    CompletedAllActions = completedAllActions;
    CommittedCount = committedCount;
  }

  /// <summary>
  /// Generation scope copied from the execution result. Null is preserved for
  /// results created through the legacy compatibility constructor.
  /// </summary>
  public long? GenerationId { get; }

  public IReadOnlyList<WorldGenerationActionExecutionRecord> Records { get; }

  public IReadOnlyList<WorldGenerationAction> UncommittedActions { get; }

  public int RecordCount => Records.Count;

  public int? FirstFailedIndex { get; }

  public string? FirstFailureReason { get; }

  public bool ContinuedAfterFailure { get; }

  public bool StoppedOnFailure { get; }

  public bool Cancelled { get; }

  public string? CancellationReason { get; }

  public bool Succeeded { get; }

  public bool CompletedAllActions { get; }

  public int CommittedCount { get; }

  public static WorldGenerationActionResultProjection From(
    WorldGenerationActionExecutionResult result)
  {
    ArgumentNullException.ThrowIfNull(result);

    WorldGenerationActionExecutionRecord[] records =
      new WorldGenerationActionExecutionRecord[result.Records.Count];
    for (int index = 0; index < records.Length; index++)
    {
      records[index] = result.Records[index];
    }

    WorldGenerationAction[] uncommittedActions =
      new WorldGenerationAction[result.UncommittedActions.Count];
    for (int index = 0; index < uncommittedActions.Length; index++)
    {
      uncommittedActions[index] = result.UncommittedActions[index];
    }

    return new WorldGenerationActionResultProjection(
      result.GenerationId,
      Array.AsReadOnly(records),
      Array.AsReadOnly(uncommittedActions),
      result.FirstFailedIndex,
      result.FirstFailureReason,
      result.ContinuedAfterFailure,
      result.StoppedOnFailure,
      result.Cancelled,
      result.CancellationReason,
      result.Succeeded,
      result.CompletedAllActions,
      result.CommittedCount);
  }
}
