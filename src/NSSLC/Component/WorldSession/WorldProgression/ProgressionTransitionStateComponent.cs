namespace Terraria.WorldProgression.Components;

/// <summary>
/// 保存世界进度转换请求的身份、阶段和变换占用。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldGen。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/WorldGen.cs。</para>
/// <para>主要源成员：TransformingWorld（第 4368 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-06-world-progression-and-transition-actual-code-component-draft.md。
/// </para>
/// <para>依据位置：第 416 行。</para>
/// </remarks>
public sealed class ProgressionTransitionStateComponent
{
  public TransitionId? TransitionId { get; set; }
  public WorldEntityId? WorldEntityId { get; set; }
  public WorldProgressionTransitionKind TransitionKind { get; set; }
  public TransitionPhase Phase { get; set; } = TransitionPhase.Idle;
  public long RequestedAtTick { get; set; }
  public PlanId? PlanId { get; set; }
  public int ActiveTransformationCount { get; set; }
  public bool IsTransforming => ActiveTransformationCount > 0;
}
