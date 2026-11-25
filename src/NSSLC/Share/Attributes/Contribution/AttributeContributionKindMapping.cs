namespace Terraria.Share.Attributes;

/// <summary>
/// 把独立管线形态映射为属性来源分类。
/// </summary>
/// <remarks>
/// 映射留在 <c>Contribution</c> 子域，因为它是十一类形态的分类法，不属于十四类共有抽象。
/// 根层的 <see cref="AttributeContributionSet" /> 只要求一个
/// <see cref="AttributeSourceDescriptor" />，不需要知道具体形态名称。
/// </remarks>
public static class AttributeContributionKindMapping
{
  public static AttributeSourceCategory ToSourceCategory(AttributeContributionKind kind)
  {
    return kind switch
    {
      AttributeContributionKind.SetBonus => AttributeSourceCategory.SetBonus,
      AttributeContributionKind.Accessory => AttributeSourceCategory.Accessory,
      AttributeContributionKind.Prefix => AttributeSourceCategory.Prefix,
      AttributeContributionKind.Imbue => AttributeSourceCategory.Imbue,
      AttributeContributionKind.Summon => AttributeSourceCategory.Pet,
      AttributeContributionKind.Mount => AttributeSourceCategory.Mount,
      AttributeContributionKind.Luck => AttributeSourceCategory.Environment,
      AttributeContributionKind.Banner => AttributeSourceCategory.Environment,
      AttributeContributionKind.Concealment => AttributeSourceCategory.StatusFlag,
      AttributeContributionKind.StatusFlag => AttributeSourceCategory.StatusFlag,
      AttributeContributionKind.Cooldown => AttributeSourceCategory.Cooldown,
      _ => throw new System.ArgumentOutOfRangeException(
          nameof(kind),
          kind,
          "Unknown contribution kind.")
    };
  }
}
