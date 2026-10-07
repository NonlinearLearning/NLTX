namespace Terraria.Player;

/// <summary>
/// 保存玩家各武器和弹药类别的伤害倍率。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：meleeDamage（第 1857 行）； magicDamage（第 1859 行）； rangedDamage（第 1861 行）； rangedMultDamage（第
/// 1863 行）； arrowDamageAdditiveStack（第 1865 行）； arrowDamage（第 1867 行）； bulletDamage（第 1869 行）；
/// rocketDamage（第 1871 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 997 行。</para>
/// </remarks>
public sealed class PlayerWeaponDamageModifierComponent
{
  public float MeleeDamage { get; internal set; } = 1f;

  public float MagicDamage { get; internal set; } = 1f;

  public float RangedDamage { get; internal set; } = 1f;

  public float RangedMultDamage { get; internal set; } = 1f;

  public float ArrowDamageAdditiveStack { get; internal set; }

  public float ArrowDamage { get; internal set; } = 1f;

  public float BulletDamage { get; internal set; } = 1f;

  public float RocketDamage { get; internal set; } = 1f;

  internal void ResetEffects()
  {
    MeleeDamage = 1f;
    MagicDamage = 1f;
    RangedDamage = 1f;
    RangedMultDamage = 1f;
    ArrowDamageAdditiveStack = 0f;
    ArrowDamage = 1f;
    BulletDamage = 1f;
    RocketDamage = 1f;
  }
}
