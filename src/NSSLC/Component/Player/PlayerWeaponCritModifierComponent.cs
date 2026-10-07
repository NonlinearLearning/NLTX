namespace Terraria.Player;

/// <summary>
/// 保存玩家各武器类别的暴击修正。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：meleeCrit（第 1851 行）； magicCrit（第 1853 行）； rangedCrit（第 1855 行）；
/// revolverCritChanceBonus（第 1877 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 999 行。</para>
/// </remarks>
public sealed class PlayerWeaponCritModifierComponent
{
  public int MeleeCrit { get; internal set; } = 4;

  public int MagicCrit { get; internal set; } = 4;

  public int RangedCrit { get; internal set; } = 4;

  public int RevolverCritChanceBonus { get; internal set; }

  internal void ResetEffects()
  {
    MeleeCrit = 4;
    MagicCrit = 4;
    RangedCrit = 4;
    RevolverCritChanceBonus = 0;
  }
}
