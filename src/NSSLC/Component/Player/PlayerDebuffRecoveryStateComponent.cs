namespace Terraria.Player;

/// <summary>
/// 保存玩家控制减益、恢复和幽灵套装效果状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：chilled（第 656 行）； dazed（第 658 行）； frozen（第 660 行）； stoned（第 662 行）； ichor（第 664 行）；
/// webbed（第 666 行）； tipsy（第 668 行）； noBuilding（第 670 行）； crimsonRegen（第 743 行）； ghostHeal（第 745
/// 行）； ghostHurt（第 747 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 78 行。</para>
/// </remarks>
public sealed class PlayerDebuffRecoveryStateComponent
{
  public bool Chilled { get; internal set; }

  public bool Dazed { get; internal set; }

  public bool Frozen { get; internal set; }

  public bool Stoned { get; internal set; }

  public bool Ichor { get; internal set; }

  public bool Webbed { get; internal set; }

  public bool Tipsy { get; internal set; }

  public bool NoBuilding { get; internal set; }

  public bool CrimsonRegen { get; internal set; }

  public bool GhostHeal { get; internal set; }

  public bool GhostHurt { get; internal set; }

  internal void ResetEffects()
  {
    Chilled = false;
    Dazed = false;
    Frozen = false;
    Stoned = false;
    Ichor = false;
    Webbed = false;
    Tipsy = false;
    NoBuilding = false;
    CrimsonRegen = false;
    GhostHeal = false;
    GhostHurt = false;
  }

  internal void ResetForLifecycle()
  {
    ResetEffects();
  }
}
