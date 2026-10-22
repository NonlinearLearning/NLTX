namespace Terraria.Player;

/// <summary>
/// 保存玩家当前坐下、睡眠等休息活动的位集。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：sitting（第 2286 行）； sleeping（第 2288 行）。</para>
/// <para>重组说明：Activities 是将坐下、睡眠等休息状态合并表达的活动位集。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P04-player-lifecycle-interaction-component-design.md。
/// </para>
/// <para>依据位置：第 70 行。</para>
/// <para>
/// 用途：休息流程汇总当前活动，再让生命恢复、时间推进或表现流程读取玩家的休息状态。
/// </para>
/// <para>
/// 当前实现：只保存 Activities。具体座位、床位关系及活动进入和退出由外部流程维护。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C141，L2（通用可选玩法）。分级理由：统计金币损失、坐下、床睡眠和休息加成是死亡重生以外的玩法。
/// </para>
/// </remarks>
public sealed class PlayerRestComponent
{
  /// <summary>
  /// 当前休息活动位集，具体位含义由 PlayerRestActivity 定义。
  /// </summary>
  public PlayerRestActivity Activities { get; internal set; }
}
