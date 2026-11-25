using System;

namespace Terraria.Share.Attributes;

/// <summary>
/// 十四类泛 Buff 形态共有的写入边界：把一组贡献登记进属性集合，或按来源整体撤销。
/// </summary>
/// <remarks>
/// 本类只做"贡献 → 修饰器"的翻译，实际增删委托给 <see cref="AttributeSystem" />，
/// 因此来源登记、过期与分类撤销只有一套规则。它不持有状态、不推进时钟、不读写 ECS。
/// </remarks>
public static class AttributeContributionSystem
{
  /// <summary>把一组贡献加入属性集合，返回实际加入的修饰器数量。</summary>
  public static int Apply(
      AttributeSetComponent attributes,
      AttributeContributionSet contributions,
      int stacks = 1)
  {
    ArgumentNullException.ThrowIfNull(attributes);
    ArgumentNullException.ThrowIfNull(contributions);

    // 同一来源重复登记时先撤销旧贡献，避免叠加出过期副本。
    AttributeSystem.RemoveSource(attributes, contributions.Source.SourceId);

    int applied = 0;
    foreach (AttributeModifier modifier in contributions.ToModifiers(stacks))
    {
      AttributeSystem.AddModifier(attributes, modifier);
      applied++;
    }

    return applied;
  }

  /// <summary>按来源撤销全部贡献，返回被移除的修饰器数量。</summary>
  public static int Revoke(AttributeSetComponent attributes, long sourceId)
  {
    return AttributeSystem.RemoveSource(attributes, sourceId);
  }

  /// <summary>按来源分类撤销全部贡献，返回被移除的修饰器数量。</summary>
  public static int RevokeCategory(
      AttributeSetComponent attributes,
      AttributeSourceCategory sourceCategory)
  {
    return AttributeSystem.RemoveSourceCategory(attributes, sourceCategory);
  }
}
