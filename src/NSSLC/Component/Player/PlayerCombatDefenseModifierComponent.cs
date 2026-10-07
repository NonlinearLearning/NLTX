namespace Terraria.Player;

/// <summary>
/// 保存玩家防御、穿甲和击退免疫输入。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：armorPenetration（第 1351 行）； meleeArmorPenetration（第 1353 行）； statDefense（第 1355 行）；
/// noKnockback（第 1383 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 605 行。</para>
/// </remarks>
public sealed class PlayerCombatDefenseModifierComponent
{
  public int ArmorPenetration { get; internal set; }

  public int MeleeArmorPenetration { get; internal set; }

  public int StatDefense { get; internal set; }

  public bool NoKnockback { get; internal set; }

  internal void ResetEffects()
  {
    ArmorPenetration = 0;
    MeleeArmorPenetration = 0;
    StatDefense = 0;
    NoKnockback = false;
  }
}
