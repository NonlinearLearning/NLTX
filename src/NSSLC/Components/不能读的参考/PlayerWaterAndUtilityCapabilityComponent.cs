namespace Terraria.Player;

/// <summary>
/// 保存玩家潜水头盔、脚蹼和药水站相关能力。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：accDivingHelm（第 2040 行）； accFlipper（第 2042 行）； deadCellsPotionStation（第 2044 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 1101 行。</para>
/// <para>
/// 用途：装备与效果流程汇总水下生存、游泳和药水站能力，环境与移动流程读取结果。
/// </para>
/// <para>
/// 当前实现：三个标记默认 false，ResetEffects 全部清空。具体呼吸规则、游泳控制和药水站行为由外部流程处理。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C148，L2（通用可选玩法）。分级理由：区域、游泳、呼吸、熔岩耐受、风力及表面状态属于额外环境玩法。
/// </para>
/// </remarks>
public sealed class PlayerWaterAndUtilityCapabilityComponent
{
  /// <summary>
  /// 潜水头盔相关能力是否生效。
  /// </summary>
  public bool AccDivingHelm { get; internal set; }

  /// <summary>
  /// 脚蹼游泳能力是否生效。
  /// </summary>
  public bool AccFlipper { get; internal set; }

  /// <summary>
  /// 原版 deadCellsPotionStation 药水站能力是否生效。
  /// </summary>
  public bool DeadCellsPotionStation { get; internal set; }

  /// <summary>
  /// 清除潜水头盔、脚蹼和药水站能力标记。
  /// </summary>
  internal void ResetEffects()
  {
    AccDivingHelm = false;
    AccFlipper = false;
    DeadCellsPotionStation = false;
  }
}
