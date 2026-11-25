using System;

namespace Terraria.Share.Movement;

/// <summary>
/// 表示一个可追踪、可过期的重力来源贡献。
/// </summary>
public readonly record struct GravityModifier
{
  public GravityModifier(
    long sourceId,
    GravityModifierKind kind,
    float value,
    int priority = 0,
    int remainingFrames = -1,
    GravityDirection direction = GravityDirection.Normal,
    long sequence = 0)
  {
    if (sourceId == 0)
    {
      throw new ArgumentOutOfRangeException(nameof(sourceId), "A gravity source must have a non-zero identity.");
    }

    if (!float.IsFinite(value))
    {
      throw new ArgumentOutOfRangeException(nameof(value), "A gravity modifier must be finite.");
    }

    bool isNonNegativeValue = kind is GravityModifierKind.MagnitudeMultiplier or
      GravityModifierKind.MaxFallSpeedMultiplier or
      GravityModifierKind.MagnitudeOverride or
      GravityModifierKind.MaxFallSpeedOverride;
    if (isNonNegativeValue && value < 0f)
    {
      throw new ArgumentOutOfRangeException(
        nameof(value),
        "Gravity multipliers and overrides cannot be negative.");
    }

    if (remainingFrames == 0 || remainingFrames < -1)
    {
      throw new ArgumentOutOfRangeException(
        nameof(remainingFrames),
        "A gravity modifier must be permanent (-1) or last for at least one frame.");
    }

    if (direction is not (GravityDirection.Normal or GravityDirection.Reversed))
    {
      throw new ArgumentOutOfRangeException(nameof(direction));
    }

    SourceId = sourceId;
    Kind = kind;
    Value = value;
    Priority = priority;
    RemainingFrames = remainingFrames;
    Direction = direction;
    Sequence = sequence;
  }

  public long SourceId { get; }

  public GravityModifierKind Kind { get; }

  public float Value { get; }

  public int Priority { get; }

  public int RemainingFrames { get; init; }

  public GravityDirection Direction { get; }

  public long Sequence { get; init; }

  public bool IsPermanent => RemainingFrames < 0;

  public GravityModifier AdvanceOneFrame()
  {
    return IsPermanent ? this : this with { RemainingFrames = RemainingFrames - 1 };
  }

    internal void Validate()
    {
        if (SourceId == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(SourceId));
        }

        if (!Enum.IsDefined(Kind))
        {
            throw new ArgumentOutOfRangeException(nameof(Kind));
        }

        if (!float.IsFinite(Value))
        {
            throw new ArgumentOutOfRangeException(nameof(Value));
        }

        bool isNonNegativeValue = Kind is GravityModifierKind.MagnitudeMultiplier or
            GravityModifierKind.MaxFallSpeedMultiplier or
            GravityModifierKind.MagnitudeOverride or
            GravityModifierKind.MaxFallSpeedOverride;
        if (isNonNegativeValue && Value < 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(Value));
        }

        if (RemainingFrames == 0 || RemainingFrames < -1)
        {
            throw new ArgumentOutOfRangeException(nameof(RemainingFrames));
        }

        if (Direction is not (GravityDirection.Normal or GravityDirection.Reversed))
        {
            throw new ArgumentOutOfRangeException(nameof(Direction));
        }
    }
}
