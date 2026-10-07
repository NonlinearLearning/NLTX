using System.Collections.Immutable;

namespace Terraria.WorldProgression.Components;

/// <summary>
/// 保存世界进度转换的批次提交状态和版本结果。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 WorldGen.StartHardmode、SmashAltar 及世界方块转换流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/WorldGen.cs。</para>
/// <para>重组说明：转换计划、分批提交、版本前提和失败恢复是流程事务化时新增的状态。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-p13-npc-town-progression-component-design.md。</para>
/// <para>依据位置：第 150 行。</para>
/// </remarks>
public sealed class ProgressionCommitStateComponent
{
  public TransitionCommitStatus CommitStatus { get; set; }
  public long CommitSequence { get; set; }
  public int AppliedBatchCount { get; set; }
  public int TotalBatchCount { get; set; }
  public WorldRevision? WorldRevisionBefore { get; set; }
  public WorldRevision? WorldRevisionAfter { get; set; }
  public ImmutableArray<WorldSectionVersion> ChangedSections { get; set; } =
    ImmutableArray<WorldSectionVersion>.Empty;
  public bool IsResultKnown { get; set; }
}
