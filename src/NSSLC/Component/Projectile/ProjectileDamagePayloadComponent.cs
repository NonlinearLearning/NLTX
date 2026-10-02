using System;

namespace Terraria.Projectile;

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
