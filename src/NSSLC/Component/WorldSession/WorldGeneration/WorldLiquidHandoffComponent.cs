using System;

namespace Terraria.WorldGeneration.Components;

public readonly record struct WorldLiquidHandoffComponent
{
  public WorldLiquidHandoffComponent(
    long generationId,
    ulong propagationRevision = 0,
    int pendingWorkItemCount = 0,
    long completedVisitCount = 0,
    bool stable = false,
    ulong? snapshotRevision = null,
    string? failureReason = null)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    if (pendingWorkItemCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(pendingWorkItemCount));
    }

    if (completedVisitCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(completedVisitCount));
    }

    if (stable && pendingWorkItemCount != 0)
    {
      throw new ArgumentException(
        "A liquid handoff cannot be stable while work items remain pending.",
        nameof(stable));
    }

    GenerationId = generationId;
    PropagationRevision = propagationRevision;
    PendingWorkItemCount = pendingWorkItemCount;
    CompletedVisitCount = completedVisitCount;
    Stable = stable;
    SnapshotRevision = snapshotRevision;
    FailureReason = failureReason;
  }

  public long GenerationId { get; }

  public ulong PropagationRevision { get; }

  public int PendingWorkItemCount { get; }

  public long CompletedVisitCount { get; }

  public bool Stable { get; }

  public ulong? SnapshotRevision { get; }

  public string? FailureReason { get; }
}
