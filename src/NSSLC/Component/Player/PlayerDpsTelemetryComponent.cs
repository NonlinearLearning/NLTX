namespace Terraria.Player;

/// <summary>
/// 保存玩家 DPS 统计的窗口、伤害和最后命中时间。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：dpsStart（第 2018 行）； dpsEnd（第 2020 行）； dpsLastHit（第 2022 行）； dpsDamage（第 2024 行）；
/// dpsStarted（第 2026 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 1150 行。</para>
/// </remarks>
public sealed class PlayerDpsTelemetryComponent
{
  public DateTimeOffset DpsStart { get; internal set; }

  public DateTimeOffset DpsEnd { get; internal set; }

  public DateTimeOffset DpsLastHit { get; internal set; }

  public int DpsDamage { get; internal set; }

  public bool DpsStarted { get; internal set; }

  internal void Reset()
  {
    DpsStart = default;
    DpsEnd = default;
    DpsLastHit = default;
    DpsDamage = 0;
    DpsStarted = false;
  }
}
