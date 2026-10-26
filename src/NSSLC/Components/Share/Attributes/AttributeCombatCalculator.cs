using System;

namespace Terraria.Attributes;

/// <summary>
/// Pure combat-facing calculations backed by <see cref="AttributeSetComponent"/>.
/// This type only resolves values; it never changes an entity or an ECS world.
/// </summary>
public static class AttributeCombatCalculator
{
  public const float DefaultCriticalMultiplier = 2f;

  public static float ResolveCriticalChance(AttributeSetComponent? attributes)
  {
    return ClampUnit(Resolve(attributes, AttributeKind.CriticalChance));
  }

  public static float ResolveCriticalMultiplier(AttributeSetComponent? attributes)
  {
    float multiplier = Resolve(
        attributes,
        AttributeKind.CriticalMultiplier,
        DefaultCriticalMultiplier);
    return float.IsFinite(multiplier) && multiplier >= 0f
        ? multiplier
        : DefaultCriticalMultiplier;
  }

  public static bool RollCritical(
      AttributeSetComponent? attributes,
      in CriticalRollInput input)
  {
    input.Validate();
    float chance = ResolveCriticalChance(attributes);
    if (chance <= 0f)
    {
      return false;
    }

    if (chance >= 1f)
    {
      return true;
    }

    ulong state = input.WorldSeed;
    state ^= unchecked((ulong)input.Tick) * 0x9E3779B97F4A7C15UL;
    state ^= unchecked((ulong)input.SourceKey) * 0xBF58476D1CE4E5B9UL;
    state ^= unchecked((ulong)input.AttackDefinitionKey) * 0x94D049BB133111EBUL;
    state ^= unchecked((ulong)input.RequestId) * 0xD2B74407B1CE6E93UL;
    ulong random = SplitMix64(state);
    double unit = (random >> 11) * (1d / (1UL << 53));
    return unit < chance;
  }

  public static float ResolveResistance(
      AttributeSetComponent? attributes,
      DamageElement element)
  {
    if (element == DamageElement.None)
    {
      return 0f;
    }

    return ClampUnit(Resolve(
        attributes,
        ResolveResistanceAttribute(element),
        fallback: 0f));
  }

  public static float ResolveElementalPenetration(AttributeSetComponent? attributes)
  {
    return Math.Max(0f, Resolve(attributes, AttributeKind.ElementalPenetration));
  }

  public static int ResolveShield(AttributeSetComponent? attributes)
  {
    // Shield is a current runtime pool. Capacity and recharge modifiers belong to MaxShield;
    // consuming this value must not reapply a modifier on every hit.
    if (attributes is null || !attributes.TryGetBaseValue(AttributeKind.Shield, out float value))
    {
      return 0;
    }

    return ToBoundedInt(value);
  }

  public static int ResolveMaxShield(AttributeSetComponent? attributes)
  {
    return ToBoundedInt(Resolve(attributes, AttributeKind.MaxShield));
  }

  public static float ResolveReflectionRatio(AttributeSetComponent? attributes)
  {
    return ClampUnit(Resolve(attributes, AttributeKind.ReflectRatio));
  }

  public static float ResolveLifeStealRatio(AttributeSetComponent? attributes)
  {
    return ClampUnit(Resolve(attributes, AttributeKind.LifeStealRatio));
  }

  public static int CalculateReflectedDamage(
      AttributeSetComponent? attributes,
      int actualLifeDamage)
  {
    if (actualLifeDamage < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(actualLifeDamage));
    }

    return ToBoundedInt(actualLifeDamage * ResolveReflectionRatio(attributes));
  }

  public static int CalculateLifeSteal(
      AttributeSetComponent? attributes,
      int actualLifeDamage)
  {
    if (actualLifeDamage < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(actualLifeDamage));
    }

    return ToBoundedInt(actualLifeDamage * ResolveLifeStealRatio(attributes));
  }

  private static float Resolve(
      AttributeSetComponent? attributes,
      AttributeKind kind,
      float fallback = 0f)
  {
    return attributes is null ? fallback : AttributeCalculator.Resolve(attributes, kind, fallback);
  }

  private static AttributeKind ResolveResistanceAttribute(DamageElement element)
  {
    return element switch
    {
      DamageElement.Fire => AttributeKind.FireResistance,
      DamageElement.Ice => AttributeKind.IceResistance,
      DamageElement.Poison => AttributeKind.PoisonResistance,
      DamageElement.Lightning => AttributeKind.LightningResistance,
      DamageElement.Holy => AttributeKind.HolyResistance,
      DamageElement.Shadow => AttributeKind.ShadowResistance,
      DamageElement.None => throw new ArgumentOutOfRangeException(nameof(element)),
      _ => throw new ArgumentOutOfRangeException(nameof(element), element, "Unknown damage element.")
    };
  }

  private static float ClampUnit(float value)
  {
    return float.IsFinite(value) ? Math.Clamp(value, 0f, 1f) : 0f;
  }

  private static int ToBoundedInt(float value)
  {
    if (!float.IsFinite(value) || value <= 0f)
    {
      return 0;
    }

    return value >= int.MaxValue ? int.MaxValue : (int)MathF.Floor(value);
  }

  private static ulong SplitMix64(ulong value)
  {
    value += 0x9E3779B97F4A7C15UL;
    value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
    value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
    return value ^ (value >> 31);
  }
}
