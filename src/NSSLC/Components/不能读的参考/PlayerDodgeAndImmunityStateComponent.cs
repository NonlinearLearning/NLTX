namespace Terraria.Player;

/// <summary>
/// 保存玩家黑腰带、混乱之脑和暗影闪避相关状态。
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
/// <para>
/// 用途：装备和战斗流程根据这些来源决定闪避资格，并记录闪避动画与暗影闪避持续计时。
/// </para>
/// <para>
/// 当前实现：只保存状态和物品引用。概率判定、是否取消本次伤害、动画播放和计时重置由外部流程完成。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C121，L2（通用可选玩法）。分级理由：穿甲、减伤、暴击、武器倍率、攻速、招架及装备闪避是基础扣血之外的规则。
/// </para>
/// </remarks>
public sealed class PlayerDodgeAndImmunityStateComponent
{
  /// <summary>
  /// 黑腰带提供的闪避能力是否生效。
  /// </summary>
  public bool BlackBelt { get; internal set; }

  /// <summary>
  /// 提供混乱之脑效果的物品实体引用，默认 None。
  /// </summary>
  public ItemEntityRef BrainOfConfusionItem { get; internal set; } =
    ItemEntityRef.None;

  /// <summary>
  /// 混乱之脑闪避动画的进度计数。
  /// </summary>
  public int BrainOfConfusionDodgeAnimationCounter { get; internal set; }

  /// <summary>
  /// 暗影闪避状态是否生效。
  /// </summary>
  public bool ShadowDodge { get; internal set; }

  /// <summary>
  /// 暗影闪避相关计时。
  /// </summary>
  public int ShadowDodgeTimer { get; internal set; }
}
