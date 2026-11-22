using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Terraria.Attributes;

/// <summary>
/// 一个来源声明的一组属性贡献：共享同一个来源身份、分类与生命周期。
/// </summary>
/// <remarks>
/// 十四类泛 Buff 形态都以"某个来源提出若干条数值贡献"的形式进入属性系统，
/// 因此可以整体加入或整体撤销，调用方不需要逐条追踪。
/// </remarks>
public sealed class AttributeContributionSet
{
  private readonly ReadOnlyCollection<AttributeContribution> _contributions;

  public AttributeContributionSet(
      AttributeSourceDescriptor source,
      IEnumerable<AttributeContribution> contributions)
  {
    ArgumentNullException.ThrowIfNull(contributions);

    Source = source;
    _contributions = new ReadOnlyCollection<AttributeContribution>(contributions.ToArray());
  }

  public AttributeSourceDescriptor Source { get; }

  public IReadOnlyList<AttributeContribution> Contributions => _contributions;

  public int Count => _contributions.Count;

  /// <summary>产出本组贡献对应的修饰器序列。</summary>
  public IEnumerable<AttributeModifier> ToModifiers(int stacks = 1)
  {
    foreach (AttributeContribution contribution in _contributions)
    {
      yield return contribution.ToModifier(
          Source.SourceCategory,
          Source.SourceId,
          stacks,
          Source.RemainingTicks);
    }
  }
}
