using System;

namespace Terraria.Attributes;

/// <summary>
/// 表示一个可追踪、可过期的属性来源贡献。
/// </summary>
public readonly record struct AttributeModifier
{
  public AttributeModifier(
    AttributeKind attribute,
    AttributeSourceCategory sourceCategory,
    AttributeModifierOperation operation,
    float value,
    long sourceId,
    int priority = 0,
    int remainingTicks = -1)
  {
    if (!Enum.IsDefined(attribute))
    {
      throw new ArgumentOutOfRangeException(
        nameof(attribute),
        attribute,
        "Unknown attribute kind.");
    }

    AttributeKindMetadata.ValidateNumericContribution(attribute);

    if (!Enum.IsDefined(sourceCategory))
    {
      throw new ArgumentOutOfRangeException(
        nameof(sourceCategory),
        sourceCategory,
        "Unknown attribute source category.");
    }

    if (!Enum.IsDefined(operation))
    {
      throw new ArgumentOutOfRangeException(
        nameof(operation),
        operation,
        "Unknown attribute modifier operation.");
    }

    if (!float.IsFinite(value))
    {
      throw new ArgumentOutOfRangeException(nameof(value), "An attribute modifier must be finite.");
    }

    if (sourceId == 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(sourceId),
        "An attribute source must have a non-zero identity.");
    }

    if (remainingTicks == 0 || remainingTicks < -1)
    {
      throw new ArgumentOutOfRangeException(
        nameof(remainingTicks),
        "A modifier must be permanent (-1) or last for at least one tick.");
    }

    Attribute = attribute;
    SourceCategory = sourceCategory;
    Operation = operation;
    Value = value;
    SourceId = sourceId;
    Priority = priority;
    RemainingTicks = remainingTicks;
  }

  public AttributeKind Attribute { get; }

  public AttributeSourceCategory SourceCategory { get; }

  public AttributeModifierOperation Operation { get; }

  public float Value { get; }

  public long SourceId { get; }

  public int Priority { get; }

  public int RemainingTicks { get; init; }

  internal long Sequence { get; init; }

  public bool IsPermanent => RemainingTicks < 0;

  public AttributeModifier AdvanceOneTick()
  {
    return IsPermanent ? this : this with { RemainingTicks = RemainingTicks - 1 };
  }

  internal void Validate()
  {
    if (!Enum.IsDefined(Attribute))
    {
      throw new ArgumentOutOfRangeException(
        nameof(Attribute),
        Attribute,
        "Unknown attribute kind.");
    }

    AttributeKindMetadata.ValidateNumericContribution(Attribute);

    if (!Enum.IsDefined(SourceCategory))
    {
      throw new ArgumentOutOfRangeException(
        nameof(SourceCategory),
        SourceCategory,
        "Unknown attribute source category.");
    }

    if (!Enum.IsDefined(Operation))
    {
      throw new ArgumentOutOfRangeException(
        nameof(Operation),
        Operation,
        "Unknown attribute modifier operation.");
    }

    if (!float.IsFinite(Value))
    {
      throw new ArgumentOutOfRangeException(nameof(Value), "An attribute modifier must be finite.");
    }

    if (SourceId == 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(SourceId),
        "An attribute source must have a non-zero identity.");
    }

    if (RemainingTicks == 0 || RemainingTicks < -1)
    {
      throw new ArgumentOutOfRangeException(nameof(RemainingTicks));
    }
  }
}
