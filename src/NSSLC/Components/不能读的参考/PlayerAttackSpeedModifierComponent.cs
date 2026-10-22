namespace Terraria.Player;

/// <summary>
/// 保存玩家近战攻击速度修正和召唤武器速度加成。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：meleeSpeed（第 1879 行）； summonerWeaponSpeedBonus（第 1881 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 1048 行。</para>
/// <para>
/// 用途：装备与状态效果先汇总速度输入，再由武器使用流程换算使用时间和攻击节奏。
/// </para>
/// <para>
/// 当前实现：MeleeSpeed 的基础值为 1，SummonerWeaponSpeedBonus 的基础值为 0；ResetEffects 恢复这两个值，具体速度公式由
/// 外部流程决定。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C109，L2（通用可选玩法）。分级理由：穿甲、减伤、暴击、武器倍率、攻速、招架及装备闪避是基础扣血之外的规则。
/// </para>
/// </remarks>
public sealed class PlayerAttackSpeedModifierComponent
{
  /// <summary>
  /// 近战速度修正输入，基础值 1。
  /// </summary>
  public float MeleeSpeed { get; internal set; } = 1f;

  /// <summary>
  /// 召唤武器速度加成输入，基础值 0。
  /// </summary>
  public float SummonerWeaponSpeedBonus { get; internal set; }

  /// <summary>
  /// 清除本轮临时效果输入并恢复基础值，供外部效果流程重新汇总。
  /// </summary>
  internal void ResetEffects()
  {
    MeleeSpeed = 1f;
    SummonerWeaponSpeedBonus = 0f;
  }
}
