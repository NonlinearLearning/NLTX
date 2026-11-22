using System;

namespace Terraria.Attributes;

/// <summary>
/// 一个属性来源的身份与生命周期：谁贡献、属于哪一类、还要存活多久。
/// </summary>
/// <remarks>
/// 十四类泛 Buff 形态共有这一组事实。把它收敛成单一值对象，可以避免每个来源各自
/// 保存一遍来源编号、分类与期限，并让 <see cref="AttributeContributionSystem" /> 与
/// <see cref="AttributeSystem" /> 用同一套规则登记和撤销贡献。
/// </remarks>
public readonly record struct AttributeSourceDescriptor
{
  /// <summary>永久来源：不随时间推进而失效。</summary>
  public const int PermanentTicks = -1;

  public AttributeSourceDescriptor(
      long sourceId,
      AttributeSourceCategory sourceCategory,
      int remainingTicks = PermanentTicks)
  {
    if (sourceId == 0)
    {
      throw new ArgumentOutOfRangeException(
          nameof(sourceId),
          "An attribute source must have a non-zero identity.");
    }

    if (!Enum.IsDefined(sourceCategory))
    {
      throw new ArgumentOutOfRangeException(
          nameof(sourceCategory),
          sourceCategory,
          "Unknown attribute source category.");
    }

    if (remainingTicks == 0 || remainingTicks < PermanentTicks)
    {
      throw new ArgumentOutOfRangeException(
          nameof(remainingTicks),
          "A source must be permanent (-1) or last for at least one tick.");
    }

    SourceId = sourceId;
    SourceCategory = sourceCategory;
    RemainingTicks = remainingTicks;
  }

  public long SourceId { get; }

  public AttributeSourceCategory SourceCategory { get; }

  public int RemainingTicks { get; }

  public bool IsPermanent => RemainingTicks < 0;

  /// <summary>
  /// 推进一个 tick。返回 false 表示本来源已到期，调用方应撤销其贡献。
  /// </summary>
  /// <remarks>
  /// 永久来源永远返回 true 并保持不变。到期状态不由本类型表示（构造时拒绝 0），
  /// 因此用返回值而不是一个"零剩余"实例来表达过期。
  /// </remarks>
  public bool TryAdvanceOneTick(out AttributeSourceDescriptor advanced)
  {
    if (IsPermanent)
    {
      advanced = this;
      return true;
    }

    int next = RemainingTicks - 1;
    if (next <= 0)
    {
      advanced = this;
      return false;
    }

    advanced = new AttributeSourceDescriptor(SourceId, SourceCategory, next);
    return true;
  }
}
