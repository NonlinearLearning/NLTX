using System;

namespace Terraria.Player;

// Stores the committed facts from the most recent player death.
// Coin drops, messaging and persistence remain external effects.
/// <summary>
/// 保存玩家最近一次死亡的事实以及 PVE、PVP 累计死亡次数。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：lostCoins（第 497 行）； lostCoinString（第 499 行）； numberOfDeathsPVE（第 507 行）；
/// numberOfDeathsPVP（第 509 行）； lastDeathPostion（第 519 行）； lastDeathTime（第 521 行）； showLastDeath（第
/// 523 行）； pvpDeath（第 921 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P04-player-lifecycle-interaction-component-design.md。
/// </para>
/// <para>依据位置：第 113 行。</para>
/// <para>
/// 用途：死亡处理提交损失金币、位置和时间后，界面、统计和存档流程读取这份记录。
/// </para>
/// <para>
/// 当前实现：只保存已经提交的事实。死亡次数递增、读取时钟、掉落金币、通知和持久化由外部生命周期流程完成。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C117，L2（通用可选玩法）。分级理由：统计金币损失、坐下、床睡眠和休息加成是死亡重生以外的玩法。
/// </para>
/// </remarks>
public sealed class PlayerDeathRecordComponent
{
  /// <summary>
  /// 最近一次死亡是否属于 PVP。
  /// </summary>
  public bool WasPvpDeath { get; internal set; }

  /// <summary>
  /// 最近一次死亡损失的金币数值。
  /// </summary>
  public long LostCoins { get; internal set; }

  /// <summary>
  /// 损失金币的格式化显示文本，默认空字符串。
  /// </summary>
  public string LostCoinText { get; internal set; } = string.Empty;

  /// <summary>
  /// 累计 PVE 死亡次数。
  /// </summary>
  public int PveDeathCount { get; internal set; }

  /// <summary>
  /// 累计 PVP 死亡次数。
  /// </summary>
  public int PvpDeathCount { get; internal set; }

  /// <summary>
  /// 最近一次死亡的世界位置。
  /// </summary>
  public WorldPosition LastDeathPosition { get; internal set; }

  /// <summary>
  /// 外部死亡流程提交的时间。
  /// </summary>
  public DateTime LastDeathTime { get; internal set; }

  /// <summary>
  /// 是否显示最近死亡位置相关信息。
  /// </summary>
  public bool ShowLastDeath { get; internal set; }
}
