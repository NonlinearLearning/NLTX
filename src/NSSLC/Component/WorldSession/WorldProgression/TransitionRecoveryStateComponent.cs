using System.Collections.Immutable;

namespace Terraria.WorldProgression.Components;

/// <summary>
/// 保存世界进度转换失败后的恢复阶段、待重试批次和未知结果。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 WorldGen.StartHardmode、SmashAltar 及世界方块转换流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/WorldGen.cs。</para>
/// <para>重组说明：转换计划、分批提交、版本前提和失败恢复是流程事务化时新增的状态。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-06-world-progression-and-transition-actual-code-component-draft.md。
/// </para>
/// <para>依据位置：第 528 行。</para>
/// </remarks>
public sealed class TransitionRecoveryStateComponent
{
  public RecoveryPhase RecoveryPhase { get; set; }
  public TransitionPhase? LastSafePhase { get; set; }
  public ImmutableArray<int> PendingBatchIndexes { get; set; } = ImmutableArray<int>.Empty;
  public TransitionFailureCode? FailureCode { get; set; }
  public int RetryCount { get; set; }
  public bool ResultUnknown { get; set; }
  public long RecoveryRevision { get; set; }
}
