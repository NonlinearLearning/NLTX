namespace Terraria.WorldProgression.Components;

/// <summary>
/// 保存世界困难模式规则及最近已提交的转换。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Main。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Main.cs。</para>
/// <para>主要源成员：hardMode（第 495 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-06-world-progression-and-transition-actual-code-component-draft.md。
/// </para>
/// <para>依据位置：第 331 行。</para>
/// </remarks>
public sealed class WorldHardmodeRuleStateComponent
{
  public bool IsHardMode { get; set; }
  public HardmodeRulePhase RulePhase { get; set; } = HardmodeRulePhase.PreHardmode;
  public TransitionId? LastCommittedTransitionId { get; set; }
  public long LastCommittedSequence { get; set; }
}
