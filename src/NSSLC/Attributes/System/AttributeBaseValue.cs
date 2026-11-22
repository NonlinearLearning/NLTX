using System;

namespace Terraria.Attributes;

/// <summary>
/// 属性的基础值。基础值不是来源修饰器，不能通过来源移除操作删除。
/// </summary>
public readonly record struct AttributeBaseValue
{
  public AttributeBaseValue(AttributeKind attribute, float value)
  {
    if (!Enum.IsDefined(attribute))
    {
      throw new ArgumentOutOfRangeException(
        nameof(attribute),
        attribute,
        "Unknown attribute kind.");
    }

    if (!float.IsFinite(value))
    {
      throw new ArgumentOutOfRangeException(
        nameof(value),
        "An attribute base value must be finite.");
    }

    Attribute = attribute;
    Value = value;
  }

  public AttributeKind Attribute { get; }

  public float Value { get; }
}
