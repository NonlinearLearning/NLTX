using System;
using Terraria.Attributes;

namespace Terraria.StatusEffects;

/// <summary>Immutable attribute contribution declared by a Buff definition.</summary>
public readonly record struct BuffAttributeModifierDefinition
{
  public BuffAttributeModifierDefinition(
      AttributeKind attribute,
      AttributeModifierOperation operation,
      float value,
      int priority = 0)
  {
    if (!Enum.IsDefined(attribute))
    {
      throw new ArgumentOutOfRangeException(nameof(attribute));
    }

    if (!Enum.IsDefined(operation))
    {
      throw new ArgumentOutOfRangeException(nameof(operation));
    }

    if (!float.IsFinite(value))
    {
      throw new ArgumentOutOfRangeException(nameof(value));
    }

    Attribute = attribute;
    Operation = operation;
    Value = value;
    Priority = priority;
  }

  public AttributeKind Attribute { get; }

  public AttributeModifierOperation Operation { get; }

  public float Value { get; }

  public int Priority { get; }
}
