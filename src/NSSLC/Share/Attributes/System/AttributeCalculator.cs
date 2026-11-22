using System;

namespace Terraria.Attributes;

/// <summary>
/// 根据组件中的基础值和修饰器计算当前属性，不执行状态写入或其他副作用。
/// </summary>
public static class AttributeCalculator
{
  public static float Resolve(
    AttributeSetComponent attributes,
    AttributeKind attribute,
    float fallback = 0f)
  {
    ArgumentNullException.ThrowIfNull(attributes);
    ValidateAttribute(attribute);

    float value = attributes.TryGetBaseValue(attribute, out float baseValue)
      ? baseValue
      : ValidateFiniteFallback(fallback);
    float additive = 0f;
    float multiplier = 1f;
    AttributeModifier? selectedOverride = null;

    foreach (AttributeModifier modifier in attributes.Modifiers)
    {
      modifier.Validate();
      if (modifier.Attribute != attribute ||
        (!modifier.IsPermanent && modifier.RemainingTicks <= 0))
      {
        continue;
      }

      switch (modifier.Operation)
      {
        case AttributeModifierOperation.Additive:
          additive += modifier.Value;
          break;
        case AttributeModifierOperation.Multiplicative:
          multiplier *= modifier.Value;
          break;
        case AttributeModifierOperation.Override:
          selectedOverride = SelectOverride(selectedOverride, modifier);
          break;
        default:
          throw new ArgumentOutOfRangeException(
            nameof(modifier.Operation),
            modifier.Operation,
            "Unknown attribute modifier operation.");
      }
    }

    value += additive;
    value *= multiplier;
    if (selectedOverride.HasValue)
    {
      value = selectedOverride.Value.Value;
    }

    if (!float.IsFinite(value))
    {
      throw new InvalidOperationException(
        $"Attribute calculation for '{attribute}' produced a non-finite value.");
    }

    return value;
  }

  public static bool TryResolve(
    AttributeSetComponent attributes,
    AttributeKind attribute,
    out float value)
  {
    ArgumentNullException.ThrowIfNull(attributes);
    ValidateAttribute(attribute);

    value = Resolve(attributes, attribute);
    return true;
  }

  private static AttributeModifier SelectOverride(
    AttributeModifier? current,
    AttributeModifier candidate)
  {
    if (!current.HasValue ||
      candidate.Priority > current.Value.Priority ||
      (candidate.Priority == current.Value.Priority &&
        candidate.Sequence > current.Value.Sequence))
    {
      return candidate;
    }

    return current.Value;
  }

  private static float ValidateFiniteFallback(float fallback)
  {
    if (!float.IsFinite(fallback))
    {
      throw new ArgumentOutOfRangeException(nameof(fallback), "A fallback value must be finite.");
    }

    return fallback;
  }

  private static void ValidateAttribute(AttributeKind attribute)
  {
    if (!Enum.IsDefined(attribute))
    {
      throw new ArgumentOutOfRangeException(
        nameof(attribute),
        attribute,
        "Unknown attribute kind.");
    }
  }
}
