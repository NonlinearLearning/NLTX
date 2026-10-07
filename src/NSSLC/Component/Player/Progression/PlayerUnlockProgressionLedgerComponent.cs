namespace Terraria.Player.Progression;

/// <summary>
/// 保存玩家生物群系火把、工匠面包和超级矿车解锁记录。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：unlockedBiomeTorches（第 1475 行）； ateArtisanBread（第 1477 行）； unlockedSuperCart（第 1479 行）；
/// enabledSuperCart（第 1481 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P05-player-progression-pets-minions-component-design.md。
/// </para>
/// <para>依据位置：第 218 行。</para>
/// </remarks>
public sealed class PlayerUnlockProgressionLedgerComponent
{
  public bool UnlockedBiomeTorches { get; internal set; }

  public bool AteArtisanBread { get; internal set; }

  public bool UnlockedSuperCart { get; internal set; }

  public bool EnabledSuperCart { get; internal set; } = true;
}
