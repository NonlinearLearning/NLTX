namespace Terraria.Player;

/// <summary>
/// 保存玩家闪避来源及闪避动画和计时。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：blackBelt（第 712 行）； brainOfConfusionItem（第 785 行）；
/// brainOfConfusionDodgeAnimationCounter（第 787 行）； shadowDodge（第 793 行）； shadowDodgeTimer（第 812
/// 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 75 行。</para>
/// </remarks>
public sealed class PlayerDodgeAndImmunityStateComponent
{
  public bool BlackBelt { get; internal set; }

  public ItemEntityRef BrainOfConfusionItem { get; internal set; } =
    ItemEntityRef.None;

  public int BrainOfConfusionDodgeAnimationCounter { get; internal set; }

  public bool ShadowDodge { get; internal set; }

  public int ShadowDodgeTimer { get; internal set; }
}
