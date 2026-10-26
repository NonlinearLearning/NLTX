using System;
using System.Collections.Generic;

namespace Terraria.Attributes;

/// <summary>
/// 管理属性修饰器的加入、来源移除和时限推进。
/// </summary>
public static class AttributeSystem
{
  /// <summary>Sets a runtime attribute value through the Attributes owner boundary.</summary>
  public static void SetBaseValue(
      AttributeSetComponent attributes,
      AttributeKind attribute,
      float value)
  {
    ArgumentNullException.ThrowIfNull(attributes);
    attributes.SetBaseValue(new AttributeBaseValue(attribute, value));
  }

  /// <summary>Consumes a shared shield pool represented by the Shield attribute.</summary>
  public static int ConsumeShield(
      AttributeSetComponent attributes,
      int requestedDamage)
  {
    ArgumentNullException.ThrowIfNull(attributes);
    if (requestedDamage < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(requestedDamage));
    }

    int current = AttributeCombatCalculator.ResolveShield(attributes);
    if (current < requestedDamage)
    {
      return 0;
    }

    int consumed = Math.Min(current, requestedDamage);
    if (consumed > 0)
    {
      SetBaseValue(attributes, AttributeKind.Shield, current - consumed);
    }

    return consumed;
  }

  public static AttributeModifier AddModifier(
    AttributeSetComponent attributes,
    AttributeModifier modifier)
  {
    ArgumentNullException.ThrowIfNull(attributes);
    return attributes.AddModifier(modifier);
  }

  public static int RemoveSource(AttributeSetComponent attributes, long sourceId)
  {
    ArgumentNullException.ThrowIfNull(attributes);
    return attributes.RemoveBySource(sourceId);
  }

  public static int RemoveSourceCategory(
    AttributeSetComponent attributes,
    AttributeSourceCategory sourceCategory)
  {
    ArgumentNullException.ThrowIfNull(attributes);
    return attributes.RemoveBySourceCategory(sourceCategory);
  }

  public static int Advance(AttributeSetComponent attributes)
  {
    ArgumentNullException.ThrowIfNull(attributes);

    List<AttributeModifier> activeModifiers = [];
    int removedCount = 0;
    foreach (AttributeModifier modifier in attributes.Modifiers)
    {
      AttributeModifier advanced = modifier.AdvanceOneTick();
      if (!advanced.IsPermanent && advanced.RemainingTicks <= 0)
      {
        removedCount++;
        continue;
      }

      activeModifiers.Add(advanced);
    }

    attributes.ReplaceModifiers(activeModifiers);
    return removedCount;
  }
}
