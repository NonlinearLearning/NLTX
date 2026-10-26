using System;
using Terraria.Attributes;

namespace Terraria.Item;

/// <summary>
/// Describes one numeric attribute contribution supplied by an item definition.
/// </summary>
public readonly record struct ItemAttributeModifierDefinition
{
  public ItemAttributeModifierDefinition(
      AttributeKind attribute,
      AttributeSourceCategory sourceCategory,
      AttributeModifierOperation operation,
      float value,
      int priority = 0)
  {
    if (!Enum.IsDefined(attribute))
    {
      throw new ArgumentOutOfRangeException(nameof(attribute));
    }

    if (!Enum.IsDefined(sourceCategory))
    {
      throw new ArgumentOutOfRangeException(nameof(sourceCategory));
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
    SourceCategory = sourceCategory;
    Operation = operation;
    Value = value;
    Priority = priority;
  }

  public AttributeKind Attribute { get; }

  public AttributeSourceCategory SourceCategory { get; }

  public AttributeModifierOperation Operation { get; }

  public float Value { get; }

  public int Priority { get; }

  internal bool IsInventorySource => SourceCategory is
      AttributeSourceCategory.Equipment or
      AttributeSourceCategory.Accessory or
      AttributeSourceCategory.Weapon;
}
