namespace Terraria.Player;

/// <summary>
/// 保存玩家物品拾取、保护小动物和保护自然等交互策略标记。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：preventAllItemPickups（第 947 行）； dontHurtCritters（第 949 行）； hasLucyTheAxe（第 951 行）；
/// dontHurtNature（第 953 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 479 行。</para>
/// <para>
/// 用途：拾取和攻击流程读取这些策略，决定是否允许拾取物品、伤害小动物或破坏相关自然内容。
/// </para>
/// <para>
/// 当前实现：只保存策略是否生效，ResetEffects 全部清为 false。具体对象分类、攻击限制和露西斧行为由外部流程执行。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C128，L2（通用可选玩法）。分级理由：该类型当前无生产消费者。禁止拾取、持物权限、自然保护和自动涂漆是默认允许规则之外的可选策
/// 略。
/// </para>
/// </remarks>
public sealed class PlayerInteractionPolicyComponent
{
  /// <summary>
  /// 是否禁止全部物品拾取。
  /// </summary>
  public bool PreventAllItemPickups { get; internal set; }

  /// <summary>
  /// 是否启用保护小动物的交互规则。
  /// </summary>
  public bool DontHurtCritters { get; internal set; }

  /// <summary>
  /// 是否具有露西斧相关交互能力。
  /// </summary>
  public bool HasLucyTheAxe { get; internal set; }

  /// <summary>
  /// 是否启用保护自然的交互规则。
  /// </summary>
  public bool DontHurtNature { get; internal set; }

  /// <summary>
  /// 清除本轮临时效果输入并恢复基础值，供外部效果流程重新汇总。
  /// </summary>
  internal void ResetEffects()
  {
    PreventAllItemPickups = false;
    DontHurtCritters = false;
    HasLucyTheAxe = false;
    DontHurtNature = false;
  }
}
