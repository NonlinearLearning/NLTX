namespace Terraria.Player;

/// <summary>
/// 保存玩家常见减益效果的直接标记。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：cursed（第 1757 行）； bleed（第 1785 行）； confused（第 1787 行）； brokenArmor（第 1795 行）； silence（第
/// 1797 行）； slow（第 1799 行）； gross（第 1801 行）； tongued（第 1803 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 804 行。</para>
/// </remarks>
public sealed class PlayerDebuffStatusAliasComponent
{
  public bool Cursed { get; internal set; }

  public bool Bleed { get; internal set; }

  public bool Confused { get; internal set; }

  public bool BrokenArmor { get; internal set; }

  public bool Silence { get; internal set; }

  public bool Slow { get; internal set; }

  public bool Gross { get; internal set; }

  public bool Tongued { get; internal set; }

  public void CommitGross(bool gross)
  {
    Gross = gross;
  }

  internal void ResetEffects()
  {
    Cursed = false;
    Bleed = false;
    Confused = false;
    BrokenArmor = false;
    Silence = false;
    Slow = false;
    Gross = false;
    Tongued = false;
  }
}
