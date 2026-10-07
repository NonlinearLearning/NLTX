using System;

namespace Terraria.Player;

// Stores the committed facts from the most recent player death.
// Coin drops, messaging and persistence remain external effects.
/// <summary>
/// 保存玩家死亡计数、金币损失和最后死亡位置时间。
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
/// </remarks>
public sealed class PlayerDeathRecordComponent
{
  public bool WasPvpDeath { get; internal set; }

  public long LostCoins { get; internal set; }

  public string LostCoinText { get; internal set; } = string.Empty;

  public int PveDeathCount { get; internal set; }

  public int PvpDeathCount { get; internal set; }

  public WorldPosition LastDeathPosition { get; internal set; }

  public DateTime LastDeathTime { get; internal set; }

  public bool ShowLastDeath { get; internal set; }
}
