using System;

namespace Terraria.Attributes;

/// <summary>
/// 一条属性贡献的最小声明：向某个属性提供一次数值修饰。
/// </summary>
/// <remarks>
/// 这是十四类泛 Buff 形态共有的最小单位。它只描述"贡献什么"，不含来源身份、
/// 来源分类与剩余期限；这些由 <see cref="AttributeSourceDescriptor" /> 在应用时补齐。
/// 定义侧（Buff 内容、物品模板、独立管线来源）都使用本类型，不再各自复制一份形状相同的定义。
/// </remarks>
public readonly record struct AttributeContribution
{
  public AttributeContribution(
      AttributeKind attribute,
      AttributeModifierOperation operation,
      float value,
      int priority = 0)
  {
    if (!Enum.IsDefined(attribute))
    {
      throw new ArgumentOutOfRangeException(
          nameof(attribute),
          attribute,
          "Unknown attribute kind.");
    }

    AttributeKindMetadata.ValidateNumericContribution(attribute);

    if (!Enum.IsDefined(operation))
    {
      throw new ArgumentOutOfRangeException(
          nameof(operation),
          operation,
          "Unknown attribute modifier operation.");
    }

    if (!float.IsFinite(value))
    {
      throw new ArgumentOutOfRangeException(nameof(value), "A contribution value must be finite.");
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

  /// <summary>把本贡献绑定到具体来源，生成可加入属性集合的修饰器。</summary>
  public AttributeModifier ToModifier(
      AttributeSourceCategory sourceCategory,
      long sourceId,
      int stacks = 1,
      int remainingTicks = -1)
  {
    if (stacks <= 0)
    {
      throw new ArgumentOutOfRangeException(
          nameof(stacks),
          "A contribution requires at least one stack.");
    }

    return new AttributeModifier(
        Attribute,
        sourceCategory,
        Operation,
        Value * stacks,
        sourceId,
        Priority,
        remainingTicks);
  }
}
