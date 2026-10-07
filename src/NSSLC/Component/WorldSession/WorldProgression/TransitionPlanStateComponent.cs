using System.Collections.Immutable;

namespace Terraria.WorldProgression.Components;

/// <summary>
/// 保存世界进度转换计划、区块前提、随机游标和矿石候选。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 WorldGen.StartHardmode、SmashAltar 及世界方块转换流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/WorldGen.cs。</para>
/// <para>重组说明：转换计划、分批提交、版本前提和失败恢复是流程事务化时新增的状态。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-06-world-progression-and-transition-actual-code-component-draft.md。
/// </para>
/// <para>依据位置：第 452 行。</para>
/// </remarks>
public sealed class TransitionPlanStateComponent
{
  public PlanId PlanId { get; set; }
  public TransitionId TransitionId { get; set; }
  public WorldRevision? BaseWorldRevision { get; set; }
  public ulong BaseGenerationRevision { get; set; }
  public ulong RandomCursor { get; set; }
  public ImmutableArray<WorldSectionId> AffectedSections { get; set; } =
    ImmutableArray<WorldSectionId>.Empty;
  public ImmutableArray<TransitionSectionPrecondition> SectionPreconditions { get; set; } =
    ImmutableArray<TransitionSectionPrecondition>.Empty;
  public int ExpectedBatchCount { get; set; }
  public HardmodeOreTierState OreTierCandidate { get; set; } = HardmodeOreTierState.Uninitialized;
  public bool RequiresResync { get; set; }
}
