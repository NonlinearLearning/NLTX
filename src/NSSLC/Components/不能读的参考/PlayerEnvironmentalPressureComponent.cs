namespace Terraria.Player;

/// <summary>
/// 保存玩家受风推动的标记和日照灼伤进度。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：windPushed（第 1765 行）； sunScorchCounter（第 1779 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 756 行。</para>
/// <para>
/// 用途：移动流程读取风推动事实，环境流程维护 SunScorchCounter；原版计数用于吸血鬼在阳光下灼烧的进度。
/// </para>
/// <para>
/// 状态变化：ResetEffects 只清除 WindPushed，保留日照计数；ResetForLifecycle 同时清除标记和计数。风力、灼烧伤害及表现由外部流程处
/// 理。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C124，L2（通用可选玩法）。分级理由：区域、游泳、呼吸、熔岩耐受、风力及表面状态属于额外环境玩法。
/// </para>
/// </remarks>
public sealed class PlayerEnvironmentalPressureComponent
{
  /// <summary>
  /// 玩家是否受到风推动。
  /// </summary>
  public bool WindPushed { get; internal set; }

  /// <summary>
  /// 日照灼伤累积进度，由外部环境流程更新。
  /// </summary>
  public int SunScorchCounter { get; internal set; }

  /// <summary>
  /// 仅清除本轮风推动标记，保留日照灼伤进度。
  /// </summary>
  internal void ResetEffects()
  {
    WindPushed = false;
  }

  /// <summary>
  /// 清除风推动标记，并将日照灼伤进度归零。
  /// </summary>
  internal void ResetForLifecycle()
  {
    WindPushed = false;
    SunScorchCounter = 0;
  }
}
