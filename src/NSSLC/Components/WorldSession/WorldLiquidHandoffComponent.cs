using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界生成向后续流程交接液体传播状态的结果。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Liquid。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Liquid.cs。</para>
/// <para>主要源成员：numLiquid（第 28 行）。</para>
/// <para>重组说明：GenerationId、PropagationRevision、SnapshotRevision 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-world-generation-and-ecology-component-design.md。</para>
/// <para>依据位置：第 660 行。</para>
/// <para>
/// 用途：生成流程读取液体传播版本、待处理工作数、已完成访问次数和稳定标记，决定后续交接条件。
/// </para>
/// <para>
/// 当前实现：构造时要求会话与计数非负，Stable 为 true 时待处理工作数必须为零。快照版本和失败原因独立保存，实际液体推进和交接判断由外部流程完成。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C199，L2（通用可选玩法）。分级理由：只有启用液体传播才需要稳定交接；关闭液体时应调整发布条件。
/// </para>
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

  /// <summary>
  /// 所属世界生成会话编号。
  /// </summary>
  public long GenerationId { get; }

  /// <summary>
  /// 此液体传播结果的版本。
  /// </summary>
  public ulong PropagationRevision { get; }

  /// <summary>
  /// 交接时仍待处理的液体工作数量。
  /// </summary>
  public int PendingWorkItemCount { get; }

  /// <summary>
  /// 已完成的液体工作访问次数。
  /// </summary>
  public long CompletedVisitCount { get; }

  /// <summary>
  /// 提交的稳定标记；为 true 时必须没有待处理工作。
  /// </summary>
  public bool Stable { get; }

  /// <summary>
  /// 此交接结果对应的快照版本，可为空。
  /// </summary>
  public ulong? SnapshotRevision { get; }

  /// <summary>
  /// 液体传播或交接失败原因，可为空。
  /// </summary>
  public string? FailureReason { get; }
}
