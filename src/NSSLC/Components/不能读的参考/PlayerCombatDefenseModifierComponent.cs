namespace Terraria.Player;

/// <summary>
/// 保存玩家穿甲、防御值和击退免疫的效果输入。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：armorPenetration（第 1351 行）； meleeArmorPenetration（第 1353 行）； statDefense（第 1355 行）；
/// noKnockback（第 1383 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 605 行。</para>
/// <para>
/// 用途：装备和状态效果汇总战斗属性后，攻击结算读取穿甲，受伤与击退结算读取防御和免疫标记。
/// </para>
/// <para>
/// 当前实现：仅保存输入。ResetEffects 将三个整数清零并关闭击退免疫；实际伤害和速度变化由外部战斗流程计算。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C114，L2（通用可选玩法）。分级理由：穿甲、减伤、暴击、武器倍率、攻速、招架及装备闪避是基础扣血之外的规则。
/// </para>
/// </remarks>
public sealed class PlayerCombatDefenseModifierComponent
{
  /// <summary>
  /// 通用穿甲输入。
  /// </summary>
  public int ArmorPenetration { get; internal set; }

  /// <summary>
  /// 近战攻击专用穿甲输入。
  /// </summary>
  public int MeleeArmorPenetration { get; internal set; }

  /// <summary>
  /// 汇总后的防御输入。
  /// </summary>
  public int StatDefense { get; internal set; }

  /// <summary>
  /// 是否启用击退免疫。
  /// </summary>
  public bool NoKnockback { get; internal set; }

  /// <summary>
  /// 清除本轮临时效果输入并恢复基础值，供外部效果流程重新汇总。
  /// </summary>
  internal void ResetEffects()
  {
    ArmorPenetration = 0;
    MeleeArmorPenetration = 0;
    StatDefense = 0;
    NoKnockback = false;
  }
}
