using System;

namespace Terraria.Projectile;

/// <summary>
/// 保存射弹伤害值、伤害类别和标签等命中载荷。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Projectile。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Projectile.cs。</para>
/// <para>
/// 主要源成员：originalDamage（第 144 行）； knockBack（第 152 行）； melee（第 224 行）； ranged（第 226 行）； magic（第
/// 228 行）； tagEffectType（第 262 行）； bonusTagDamage（第 264 行）； armorPenetration（第 266 行）；
/// bonusCritChance（第 268 行）； hostileDamageScaling（第 270 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P15-projectile-component-design.md。</para>
/// <para>依据位置：第 16 行。</para>
/// </remarks>
public struct ProjectileDamagePayloadComponent
{
  public ProjectileDamagePayloadComponent(
    int currentDamage = 0,
    int originalDamage = 0,
    float knockback = 0.0f,
    int armorPenetration = 0,
    int bonusCritChance = 0,
    int bonusTagDamage = 0,
    int tagEffectType = 0,
    ProjectileDamageClass damageClass = ProjectileDamageClass.Generic,
    bool isColdDamage = false,
    bool isArrow = false,
    ProjectileHostileDamageScaling hostileDamageScaling =
      ProjectileHostileDamageScaling.Default,
    bool melee = false,
    bool ranged = false,
    bool magic = false)
  {
    if (!float.IsFinite(knockback))
    {
      throw new ArgumentOutOfRangeException(nameof(knockback));
    }

    if (armorPenetration < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(armorPenetration));
    }

    CurrentDamage = currentDamage;
    OriginalDamage = originalDamage;
    Knockback = knockback;
    ArmorPenetration = armorPenetration;
    BonusCritChance = bonusCritChance;
    BonusTagDamage = bonusTagDamage;
    TagEffectType = tagEffectType;
    DamageClass = damageClass;
    IsColdDamage = isColdDamage;
    IsArrow = isArrow;
    HostileDamageScaling = hostileDamageScaling;
    Melee = melee;
    Ranged = ranged;
    Magic = magic;
  }

  public int CurrentDamage;
  public int OriginalDamage;
  public float Knockback;
  public int ArmorPenetration;
  public int BonusCritChance;
  public int BonusTagDamage;
  public int TagEffectType;
  public ProjectileDamageClass DamageClass;
  public bool IsColdDamage;
  public bool IsArrow;
  public ProjectileHostileDamageScaling HostileDamageScaling;

  public bool Melee;

  public bool Ranged;

  public bool Magic;

  public readonly bool IsMelee => Melee || DamageClass == ProjectileDamageClass.Melee;

  public readonly bool IsRanged => Ranged || DamageClass == ProjectileDamageClass.Ranged;

  public readonly bool IsMagic => Magic || DamageClass == ProjectileDamageClass.Magic;
}
