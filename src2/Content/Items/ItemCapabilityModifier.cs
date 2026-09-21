namespace Terraria.Content.Items;

public sealed class ItemCapabilityModifier
{
  public ItemCapabilityModifier(
    int order,
    int damageDelta = 0,
    float knockbackDelta = 0f,
    int criticalChanceDelta = 0,
    int armorPenetrationDelta = 0,
    int bonusTagDamageDelta = 0,
    int lifeRecoveryDelta = 0,
    int manaRecoveryDelta = 0,
    int lifeRegenDelta = 0,
    int maxManaIncreaseDelta = 0,
    int manaCostDelta = 0,
    float projectileSpeedDelta = 0f,
    int rarityDelta = 0,
    ProjectileContentId? projectileTypeOverride = null,
    bool? isMeleeOverride = null,
    bool? isMagicOverride = null,
    bool? isRangedOverride = null,
    bool? isSummonOverride = null,
    bool? isSentryOverride = null)
  {
    if (order < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(order));
    }

    if (float.IsNaN(knockbackDelta) || float.IsInfinity(knockbackDelta) ||
      float.IsNaN(projectileSpeedDelta) || float.IsInfinity(projectileSpeedDelta))
    {
      throw new ArgumentOutOfRangeException(nameof(knockbackDelta));
    }

    Order = order;
    DamageDelta = damageDelta;
    KnockbackDelta = knockbackDelta;
    CriticalChanceDelta = criticalChanceDelta;
    ArmorPenetrationDelta = armorPenetrationDelta;
    BonusTagDamageDelta = bonusTagDamageDelta;
    LifeRecoveryDelta = lifeRecoveryDelta;
    ManaRecoveryDelta = manaRecoveryDelta;
    LifeRegenDelta = lifeRegenDelta;
    MaxManaIncreaseDelta = maxManaIncreaseDelta;
    ManaCostDelta = manaCostDelta;
    ProjectileSpeedDelta = projectileSpeedDelta;
    RarityDelta = rarityDelta;
    ProjectileTypeOverride = projectileTypeOverride;
    IsMeleeOverride = isMeleeOverride;
    IsMagicOverride = isMagicOverride;
    IsRangedOverride = isRangedOverride;
    IsSummonOverride = isSummonOverride;
    IsSentryOverride = isSentryOverride;
  }

  public int ArmorPenetrationDelta { get; }

  public int BonusTagDamageDelta { get; }

  public int CriticalChanceDelta { get; }

  public int DamageDelta { get; }

  public int LifeRecoveryDelta { get; }

  public int LifeRegenDelta { get; }

  public bool? IsMagicOverride { get; }

  public bool? IsMeleeOverride { get; }

  public bool? IsRangedOverride { get; }

  public bool? IsSentryOverride { get; }

  public bool? IsSummonOverride { get; }

  public int ManaCostDelta { get; }

  public int ManaRecoveryDelta { get; }

  public int MaxManaIncreaseDelta { get; }

  public float KnockbackDelta { get; }

  public int Order { get; }

  public ProjectileContentId? ProjectileTypeOverride { get; }

  public float ProjectileSpeedDelta { get; }

  public int RarityDelta { get; }
}
