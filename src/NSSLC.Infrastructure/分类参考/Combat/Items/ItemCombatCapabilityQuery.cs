using Terraria.Content.Items;

namespace Terraria.Combat.Items;

public static class ItemCombatCapabilityQuery
{
  public static ItemCapabilitySnapshot Merge(
    ItemCapabilitySnapshot baseSnapshot,
    IEnumerable<ItemCapabilityModifier> modifiers)
  {
    ArgumentNullException.ThrowIfNull(baseSnapshot);
    ArgumentNullException.ThrowIfNull(modifiers);

    int damage = baseSnapshot.Combat.BaseDamage;
    float knockback = baseSnapshot.Combat.BaseKnockback;
    int criticalChance = baseSnapshot.Combat.CriticalChance;
    int armorPenetration = baseSnapshot.Combat.ArmorPenetration;
    int bonusTagDamage = baseSnapshot.Combat.BonusTagDamage;
    int lifeRecovery = baseSnapshot.ResourceRecovery.LifeRecovery;
    int manaRecovery = baseSnapshot.ResourceRecovery.ManaRecovery;
    int lifeRegen = baseSnapshot.ResourceRecovery.LifeRegen;
    int maxManaIncrease = baseSnapshot.ResourceRecovery.MaxManaIncrease;
    int manaCost = baseSnapshot.ResourceRecovery.ManaCost;
    float projectileSpeed = baseSnapshot.ProjectileUse.ProjectileSpeed;
    int rarityTier = baseSnapshot.Presentation.RarityTier;
    ProjectileContentId projectileTypeId = baseSnapshot.ProjectileUse.ProjectileTypeId;
    bool isMelee = baseSnapshot.DamageClass.IsMelee;
    bool isMagic = baseSnapshot.DamageClass.IsMagic;
    bool isRanged = baseSnapshot.DamageClass.IsRanged;
    bool isSummon = baseSnapshot.DamageClass.IsSummon;
    bool isSentry = baseSnapshot.DamageClass.IsSentry;

    ItemCapabilityModifier[] orderedModifiers = modifiers
      .OrderBy(modifier => modifier.Order)
      .ToArray();
    if (orderedModifiers.Select(modifier => modifier.Order).Distinct().Count() !=
      orderedModifiers.Length)
    {
      throw new ArgumentException(
        "Capability modifier order values must be unique.",
        nameof(modifiers));
    }

    foreach (ItemCapabilityModifier modifier in orderedModifiers)
    {
      damage = checked(damage + modifier.DamageDelta);
      knockback += modifier.KnockbackDelta;
      criticalChance = checked(criticalChance + modifier.CriticalChanceDelta);
      armorPenetration = checked(armorPenetration + modifier.ArmorPenetrationDelta);
      bonusTagDamage = checked(bonusTagDamage + modifier.BonusTagDamageDelta);
      lifeRecovery = checked(lifeRecovery + modifier.LifeRecoveryDelta);
      manaRecovery = checked(manaRecovery + modifier.ManaRecoveryDelta);
      lifeRegen = checked(lifeRegen + modifier.LifeRegenDelta);
      maxManaIncrease = checked(maxManaIncrease + modifier.MaxManaIncreaseDelta);
      manaCost = checked(manaCost + modifier.ManaCostDelta);
      projectileSpeed += modifier.ProjectileSpeedDelta;
      rarityTier = checked(rarityTier + modifier.RarityDelta);

      if (modifier.ProjectileTypeOverride is ProjectileContentId overrideType)
      {
        projectileTypeId = overrideType;
      }

      if (modifier.IsMeleeOverride is bool melee)
      {
        isMelee = melee;
      }

      if (modifier.IsMagicOverride is bool magic)
      {
        isMagic = magic;
      }

      if (modifier.IsRangedOverride is bool ranged)
      {
        isRanged = ranged;
      }

      if (modifier.IsSummonOverride is bool summon)
      {
        isSummon = summon;
      }

      if (modifier.IsSentryOverride is bool sentry)
      {
        isSentry = sentry;
      }
    }

    return new ItemCapabilitySnapshot(
      new ItemCombatCapabilityDefinition(
        damage,
        ValidateFiniteNonNegative(knockback, nameof(knockback)),
        criticalChance,
        armorPenetration,
        bonusTagDamage),
      new ItemProjectileUseDefinition(
        projectileTypeId,
        ValidateFiniteNonNegative(projectileSpeed, nameof(projectileSpeed))),
      new ItemResourceRecoveryDefinition(
        lifeRecovery,
        manaRecovery,
        lifeRegen,
        maxManaIncrease,
        manaCost),
      new ItemDamageClassCapability(isMelee, isMagic, isRanged, isSummon, isSentry),
      new ItemPresentationDefinition(rarityTier));
  }

  private static float ValidateFiniteNonNegative(float value, string parameterName)
  {
    if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }

    return value;
  }
}
