using System;
using Terraria.Attributes;

namespace Terraria.Combat;

/// <summary>Calculates damage after a bounded endurance reduction.</summary>
public sealed class DamageMitigationSystem
{
  private const float DefenseDamageFactor = 0.5f;

  /// <summary>
  /// Calculates one complete shared payload. Source attributes provide offensive modifiers;
  /// target attributes provide the existing bounded mitigation inputs.
  /// </summary>
  public DamageMitigationResult Calculate(
      in DamagePayload payload,
      AttributeSetComponent? sourceAttributes,
      AttributeSetComponent? targetAttributes,
      LifeComponent? targetLife = null)
  {
    if (!payload.IsValid)
    {
      throw new ArgumentException("The damage payload is invalid.", nameof(payload));
    }

    float damageMultiplier = 1f;
    if (sourceAttributes is not null)
    {
      damageMultiplier += AttributeCalculator.Resolve(
          sourceAttributes, AttributeKind.Damage);
      AttributeKind? typeAttribute = ResolveTypeAttribute(payload.Type);
      if (typeAttribute.HasValue)
      {
        damageMultiplier += AttributeCalculator.Resolve(sourceAttributes, typeAttribute.Value);
      }
    }

    bool critical = payload.Critical;
    if (!critical && payload.CriticalRollInput.HasValue)
    {
      critical = AttributeCombatCalculator.RollCritical(
          sourceAttributes,
          payload.CriticalRollInput.Value);
    }

    float criticalMultiplier = critical
        ? AttributeCombatCalculator.ResolveCriticalMultiplier(sourceAttributes)
        : 1f;
    float rawDamage = payload.BaseDamage * Math.Max(0f, damageMultiplier) * criticalMultiplier;
    int incomingDamage = ToBoundedInt(rawDamage);
    int afterDefenseDamage = incomingDamage;
    int afterResistanceDamage = incomingDamage;
    int shieldAbsorbedDamage = 0;
    int lifeDamage = incomingDamage;
    float reduction = 0f;

    if (payload.Type != DamageType.True && targetAttributes is not null)
    {
      float armorPenetration = payload.ArmorPenetration;
      if (sourceAttributes is not null)
      {
        armorPenetration += AttributeCalculator.Resolve(
            sourceAttributes,
            AttributeKind.ArmorPenetration);
      }

      float defense = Math.Max(0f, AttributeCalculator.Resolve(
          targetAttributes,
          AttributeKind.Defense) - Math.Max(0f, armorPenetration));
      float afterDefense = Math.Max(
          0f,
          incomingDamage - defense * DefenseDamageFactor);
      afterDefenseDamage = ToBoundedInt(afterDefense);

      float elementalPenetration = payload.ElementalPenetration +
          (sourceAttributes is null
              ? 0f
              : AttributeCombatCalculator.ResolveElementalPenetration(sourceAttributes));
      float resistance = Math.Clamp(
          AttributeCombatCalculator.ResolveResistance(targetAttributes, payload.Element) -
          Math.Max(0f, elementalPenetration),
          0f,
          1f);
      afterResistanceDamage = ToBoundedInt(afterDefenseDamage * (1f - resistance));
      reduction = AttributeCalculator.Resolve(targetAttributes, AttributeKind.DamageReduction) +
          AttributeCalculator.Resolve(targetAttributes, AttributeKind.Endurance);
      lifeDamage = ToBoundedInt(afterResistanceDamage * (1f - Math.Clamp(reduction, 0f, 1f)));
    }
    else
    {
      lifeDamage = incomingDamage;
    }

    int currentShield = AttributeCombatCalculator.ResolveShield(targetAttributes);
    shieldAbsorbedDamage = Math.Min(currentShield, lifeDamage);
    lifeDamage -= shieldAbsorbedDamage;
    if (targetLife.HasValue)
    {
      LifeComponent availableLife = targetLife.Value;
      int availableDamage = Math.Max(0, availableLife.Current - availableLife.Minimum);
      lifeDamage = Math.Min(lifeDamage, availableDamage);
    }
    int reflectedDamage = AttributeCombatCalculator.CalculateReflectedDamage(
        targetAttributes,
        lifeDamage);
    int lifeSteal = AttributeCombatCalculator.CalculateLifeSteal(
        sourceAttributes,
        lifeDamage);
    return new DamageMitigationResult(
        incomingDamage,
        lifeDamage,
        Math.Clamp(reduction, 0f, 1f),
        afterDefenseDamage,
        afterResistanceDamage,
        shieldAbsorbedDamage,
        lifeDamage,
        reflectedDamage,
        lifeSteal,
        critical);
  }

  public DamageMitigationResult Calculate(
      int incomingDamage,
      float reduction)
  {
    if (incomingDamage < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(incomingDamage));
    }

    if (float.IsNaN(reduction))
    {
      throw new ArgumentOutOfRangeException(nameof(reduction));
    }

    reduction = Math.Clamp(reduction, 0f, 1f);
    float acceptedValue = incomingDamage * (1f - reduction);
    int accepted = acceptedValue >= int.MaxValue
        ? int.MaxValue
        : (int)MathF.Floor(acceptedValue);
    return new DamageMitigationResult(
        incomingDamage,
        Math.Max(0, accepted),
        reduction,
        incomingDamage,
        incomingDamage,
        0,
        Math.Max(0, accepted));
  }

  /// <summary>Resolves shared numeric mitigation attributes for one target.</summary>
  public DamageMitigationResult Calculate(
      int incomingDamage,
      AttributeSetComponent? attributes)
  {
    return Calculate(
        incomingDamage,
        attributes is null
            ? 0f
            : AttributeCalculator.Resolve(attributes, AttributeKind.DamageReduction) +
              AttributeCalculator.Resolve(attributes, AttributeKind.Endurance));
  }

  private static AttributeKind? ResolveTypeAttribute(DamageType type)
  {
    return type switch
    {
      DamageType.Melee => AttributeKind.MeleeDamage,
      DamageType.Ranged => AttributeKind.RangedDamage,
      DamageType.Magic => AttributeKind.MagicDamage,
      DamageType.Summon => AttributeKind.SummonDamage,
      _ => null
    };
  }

  private static int ToBoundedInt(float value)
  {
    if (!float.IsFinite(value) || value <= 0f)
    {
      return 0;
    }

    return value >= int.MaxValue ? int.MaxValue : (int)MathF.Floor(value);
  }
}
