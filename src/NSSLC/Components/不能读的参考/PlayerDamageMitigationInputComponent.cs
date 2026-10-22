namespace Terraria.Player;

/// <summary>
/// 保存玩家伤害结算使用的减伤比例输入。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：endurance（第 767 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 435 行。</para>
/// <para>
/// 用途：装备和状态效果把减伤汇总为 Endurance，再由受伤结算流程应用对应比例。
/// </para>
/// <para>
/// 当前实现：默认 0，ResetEffects 清零。当前属性没有比例范围校验，最终伤害和叠加公式由外部战斗流程决定。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C116，L2（通用可选玩法）。分级理由：穿甲、减伤、暴击、武器倍率、攻速、招架及装备闪避是基础扣血之外的规则。
/// </para>
/// </remarks>
public sealed class PlayerDamageMitigationInputComponent
{
  /// <summary>
  /// 减伤比例输入，基础值 0。
  /// </summary>
  public float Endurance { get; internal set; }

  /// <summary>
  /// 清除本轮临时效果输入并恢复基础值，供外部效果流程重新汇总。
  /// </summary>
  internal void ResetEffects()
  {
    Endurance = 0f;
  }
}
