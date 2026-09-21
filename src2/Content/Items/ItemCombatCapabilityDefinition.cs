namespace Terraria.Content.Items;

public sealed record ItemCombatCapabilityDefinition
{
  public ItemCombatCapabilityDefinition(
    int baseDamage,
    float baseKnockback,
    int criticalChance,
    int armorPenetration,
    int bonusTagDamage)
  {
    if (baseDamage < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(baseDamage));
    }

    if (float.IsNaN(baseKnockback) || float.IsInfinity(baseKnockback) || baseKnockback < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(baseKnockback));
    }

    if (criticalChance < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(criticalChance));
    }

    if (armorPenetration < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(armorPenetration));
    }

    if (bonusTagDamage < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(bonusTagDamage));
    }

    BaseDamage = baseDamage;
    BaseKnockback = baseKnockback;
    CriticalChance = criticalChance;
    ArmorPenetration = armorPenetration;
    BonusTagDamage = bonusTagDamage;
  }

  public int ArmorPenetration { get; }

  public float BaseKnockback { get; }

  public int BaseDamage { get; }

  public int BonusTagDamage { get; }

  public int CriticalChance { get; }
}
