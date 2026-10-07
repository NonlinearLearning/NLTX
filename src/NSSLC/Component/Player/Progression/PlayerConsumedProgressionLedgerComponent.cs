namespace Terraria.Player.Progression;

/// <summary>
/// 保存玩家已使用的永久提升消耗品记录。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：usedAegisCrystal（第 525 行）； usedAegisFruit（第 527 行）； usedArcaneCrystal（第 529 行）；
/// usedGalaxyPearl（第 531 行）； usedGummyWorm（第 533 行）； usedAmbrosia（第 535 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P05-player-progression-pets-minions-component-design.md。
/// </para>
/// <para>依据位置：第 175 行。</para>
/// </remarks>
public sealed class PlayerConsumedProgressionLedgerComponent
{
  public bool UsedAegisCrystal { get; internal set; }

  public bool UsedAegisFruit { get; internal set; }

  public bool UsedArcaneCrystal { get; internal set; }

  public bool UsedGalaxyPearl { get; internal set; }

  public bool UsedGummyWorm { get; internal set; }

  public bool UsedAmbrosia { get; internal set; }
}
