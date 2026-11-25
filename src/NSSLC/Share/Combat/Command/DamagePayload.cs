using System;
using Terraria.Share.Attributes;
using Terraria.Share.Relationships;

namespace Terraria.Share.Combat;

/// <summary>Immutable input for one damage application.</summary>
public readonly record struct DamagePayload
{
  public DamagePayload(
      int baseDamage,
      DamageType type = DamageType.Generic,
      DamageRelation relation = DamageRelation.Hostile,
      float knockback = 0f,
      bool critical = false,
      float armorPenetration = 0f,
      bool allowFriendlyFire = false,
      bool allowSelfDamage = false,
      bool allowNeutralTargets = false,
      DamageElement element = DamageElement.None,
      float elementalPenetration = 0f,
      CriticalRollInput? criticalRollInput = null)
  {
    if (baseDamage < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(baseDamage));
    }

    if (!Enum.IsDefined(type) || !Enum.IsDefined(relation) || !Enum.IsDefined(element))
    {
      throw new ArgumentOutOfRangeException(nameof(type));
    }

    if (!float.IsFinite(knockback) || knockback < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(knockback));
    }

    if (!float.IsFinite(armorPenetration) || armorPenetration < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(armorPenetration));
    }

    if (!float.IsFinite(elementalPenetration) || elementalPenetration < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(elementalPenetration));
    }

    if (criticalRollInput.HasValue)
    {
      criticalRollInput.Value.Validate();
    }

    BaseDamage = baseDamage;
    Type = type;
    Relation = relation;
    Knockback = knockback;
    Critical = critical;
    ArmorPenetration = armorPenetration;
    AllowFriendlyFire = allowFriendlyFire;
    AllowSelfDamage = allowSelfDamage;
    AllowNeutralTargets = allowNeutralTargets;
    Element = element;
    ElementalPenetration = elementalPenetration;
    CriticalRollInput = criticalRollInput;
  }

  public int BaseDamage { get; }

  public DamageType Type { get; }

  public DamageRelation Relation { get; }

  public float Knockback { get; }

  public bool Critical { get; }

  public float ArmorPenetration { get; }

  public bool AllowFriendlyFire { get; }

  public bool AllowSelfDamage { get; }

  public bool AllowNeutralTargets { get; }

  public DamageElement Element { get; }

  public DamageElement DamageElement => Element;

  public float ElementalPenetration { get; }

  public CriticalRollInput? CriticalRollInput { get; }

  public bool IsValid => BaseDamage >= 0 &&
      Enum.IsDefined(Type) &&
      Enum.IsDefined(Relation) &&
      Enum.IsDefined(Element) &&
      float.IsFinite(Knockback) &&
      Knockback >= 0f &&
      float.IsFinite(ArmorPenetration) &&
      ArmorPenetration >= 0f &&
      float.IsFinite(ElementalPenetration) &&
      ElementalPenetration >= 0f &&
      (!CriticalRollInput.HasValue || CriticalRollInput.Value.IsValid);

}
