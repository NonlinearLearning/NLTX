using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界生成与液体传播之间的工作交接状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Liquid。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Liquid.cs。</para>
/// <para>主要源成员：numLiquid（第 28 行）。</para>
/// <para>重组说明：GenerationId、PropagationRevision、SnapshotRevision 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-world-generation-and-ecology-component-design.md。</para>
/// <para>依据位置：第 660 行。</para>
/// </remarks>
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
