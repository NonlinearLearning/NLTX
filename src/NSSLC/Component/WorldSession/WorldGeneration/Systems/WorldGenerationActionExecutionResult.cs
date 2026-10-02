using System;
using System.Collections.Generic;
using Terraria.WorldGeneration.Actions;

namespace Terraria.WorldGeneration.Systems;

public sealed class WorldGenerationActionExecutionResult
{
  /// <summary>
  /// Compatibility constructor for callers that already own a result but do
  /// not have a generation scope. Such results deliberately expose an
  /// unknown scope through <see cref="GenerationId"/>.
  /// </summary>
  public WorldGenerationActionExecutionResult(
    IReadOnlyList<WorldGenerationActionExecutionRecord> records,
    int? firstFailedIndex,
    bool stoppedOnFailure,
    IReadOnlyList<WorldGenerationAction>? uncommittedActions = null,
    bool cancelled = false,
    string? cancellationReason = null)
    : this(
      generationId: null,
      records,
      firstFailedIndex,
      stoppedOnFailure,
      uncommittedActions,
      cancelled,
      cancellationReason)
  {
  }

  public WorldGenerationActionExecutionResult(
    long generationId,
    IReadOnlyList<WorldGenerationActionExecutionRecord> records,
    int? firstFailedIndex,
    bool stoppedOnFailure,
    IReadOnlyList<WorldGenerationAction>? uncommittedActions = null,
    bool cancelled = false,
    string? cancellationReason = null)
    : this(
      generationId: (long?)generationId,
      records,
      firstFailedIndex,
      stoppedOnFailure,
      uncommittedActions,
      cancelled,
      cancellationReason)
  {
  }

  private WorldGenerationActionExecutionResult(
    long? generationId,
    IReadOnlyList<WorldGenerationActionExecutionRecord> records,
    int? firstFailedIndex,
    bool stoppedOnFailure,
    IReadOnlyList<WorldGenerationAction>? uncommittedActions,
    bool cancelled,
    string? cancellationReason)
  {
    if (generationId is < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    ArgumentNullException.ThrowIfNull(records);
    if (cancelled)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(cancellationReason);
    }
    else if (cancellationReason is not null)
    {
      throw new ArgumentException(
        "A cancellation reason is only valid for a cancelled result.",
        nameof(cancellationReason));
    }

    if (firstFailedIndex is int failedIndex &&
      ((uint)failedIndex >= (uint)records.Count || failedIndex < 0))
    {
      throw new ArgumentOutOfRangeException(nameof(firstFailedIndex));
    }

    if (generationId is long scopedGeneration && uncommittedActions is not null)
    {
      for (int index = 0; index < uncommittedActions.Count; index++)
      {
        if (uncommittedActions[index].GenerationId != scopedGeneration)
        {
          throw new ArgumentException(
            "An uncommitted action belongs to another generation.",
            nameof(uncommittedActions));
        }
      }
    }

    Records = new List<WorldGenerationActionExecutionRecord>(records)
      .AsReadOnly();
    GenerationId = generationId;
    FirstFailedIndex = firstFailedIndex;
    StoppedOnFailure = stoppedOnFailure;
    Cancelled = cancelled;
    CancellationReason = cancellationReason;
    UncommittedActions = uncommittedActions is null
      ? Array.Empty<WorldGenerationAction>()
      : new List<WorldGenerationAction>(uncommittedActions).AsReadOnly();
  }

  /// <summary>
  /// Generation scope of the execution. A null value means the legacy
  /// compatibility constructor was used and scope is unknown.
  /// </summary>
  public long? GenerationId { get; }

  public IReadOnlyList<WorldGenerationActionExecutionRecord> Records { get; }

  public int? FirstFailedIndex { get; }

  public bool StoppedOnFailure { get; }

  /// <summary>
  /// Whether the commit barrier stopped before all actions were attempted
  /// because cancellation was requested.
  /// </summary>
  public bool Cancelled { get; }

  public string? CancellationReason { get; }

  public IReadOnlyList<WorldGenerationAction> UncommittedActions { get; }

  public string? FirstFailureReason =>
    FirstFailedIndex is int index &&
    (uint)index < (uint)Records.Count
      ? Records[index].RejectionReason
      : null;

  public bool ContinuedAfterFailure =>
    FirstFailedIndex is not null && !StoppedOnFailure;

  public int CommittedCount
  {
    get
    {
      int count = 0;
      for (int index = 0; index < Records.Count; index++)
      {
        if (Records[index].Accepted)
        {
          count++;
        }
      }

      return count;
    }
  }

  public bool Succeeded => !Cancelled && FirstFailedIndex is null;

  public bool CompletedAllActions =>
    !Cancelled && !StoppedOnFailure && Records.Count > 0
      ? FirstFailedIndex is null || Records[^1].Accepted
      : !Cancelled && !StoppedOnFailure;
}
