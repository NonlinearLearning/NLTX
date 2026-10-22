namespace Terraria.Player;

/// <summary>
/// 保存玩家近战、魔法、远程暴击输入及左轮额外暴击加成。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：meleeCrit（第 1851 行）； magicCrit（第 1853 行）； rangedCrit（第 1855 行）；
/// revolverCritChanceBonus（第 1877 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 999 行。</para>
/// <para>
/// 用途：装备与状态效果汇总各类别暴击数值，武器攻击流程根据武器类别选择输入并进行概率判定。
/// </para>
/// <para>
/// 当前实现：三种类别基础值为 4，左轮额外加成默认 0。ResetEffects 恢复这些值；概率组合和暴击伤害由外部战斗流程计算。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C149，L2（通用可选玩法）。分级理由：穿甲、减伤、暴击、武器倍率、攻速、招架及装备闪避是基础扣血之外的规则。
/// </para>
/// </remarks>
public sealed class PlayerWeaponCritModifierComponent
{
  /// <summary>
  /// 近战暴击输入，基础值 4。
  /// </summary>
  public int MeleeCrit { get; internal set; } = 4;

  /// <summary>
  /// 魔法暴击输入，基础值 4。
  /// </summary>
  public int MagicCrit { get; internal set; } = 4;

  /// <summary>
  /// 远程暴击输入，基础值 4。
  /// </summary>
  public int RangedCrit { get; internal set; } = 4;

  /// <summary>
  /// 左轮武器额外暴击机会输入，基础值 0。
  /// </summary>
  public int RevolverCritChanceBonus { get; internal set; }

  /// <summary>
  /// 将三种类别暴击输入恢复为 4，并将左轮额外加成清零。
  /// </summary>
  internal void ResetEffects()
  {
    MeleeCrit = 4;
    MagicCrit = 4;
    RangedCrit = 4;
    RevolverCritChanceBonus = 0;
  }
}
