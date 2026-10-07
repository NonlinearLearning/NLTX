namespace Terraria.Player;

/// <summary>
/// 保存玩家侦测、受击响应和战斗状态效果。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：dangerSense（第 763 行）； loveStruck（第 773 行）； stinky（第 775 行）； resistCold（第 777 行）；
/// electrified（第 779 行）； dryadWard（第 781 行）； panic（第 783 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 79 行。</para>
/// </remarks>
public sealed class PlayerCombatDetectionStateComponent
{
  public bool DangerSense { get; internal set; }

  public bool LoveStruck { get; internal set; }

  public bool Stinky { get; internal set; }

  public bool ResistCold { get; internal set; }

  public bool Electrified { get; internal set; }

  public bool DryadWard { get; internal set; }

  public bool Panic { get; internal set; }

  internal void ResetEffects()
  {
    DangerSense = false;
    LoveStruck = false;
    Stinky = false;
    ResistCold = false;
    Electrified = false;
    DryadWard = false;
    Panic = false;
  }
}
