using NLTX.EconomyCraftingFishingLoot.Loot;

namespace NLTX.EconomyCraftingFishingLoot.Content;

public sealed class DropModeOptionSelector
{
  public DropModeOptionSelector(
    DropRuleReference normalRule,
    DropRuleReference? expertRule = null,
    DropRuleReference? masterRule = null,
    DropRuleReference? extraGelRule = null)
  {
    NormalRule = normalRule;
    ExpertRule = expertRule;
    MasterRule = masterRule;
    ExtraGelRule = extraGelRule;
  }

  public DropRuleReference NormalRule { get; }

  public DropRuleReference? ExpertRule { get; }

  public DropRuleReference? MasterRule { get; }

  public DropRuleReference? ExtraGelRule { get; }

  public static DropModeOptionSelector ForExpertMode(
    DropRuleReference normalRule,
    DropRuleReference expertRule)
  {
    return new DropModeOptionSelector(normalRule, expertRule);
  }

  public DropRuleReference Select(DropResolutionContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    if (context.IsMasterMode && MasterRule.HasValue)
    {
      return MasterRule.Value;
    }

    if (context.IsExpertMode && ExpertRule.HasValue)
    {
      return ExpertRule.Value;
    }

    if (context.ShouldDropExtraGel && ExtraGelRule.HasValue)
    {
      return ExtraGelRule.Value;
    }

    return NormalRule;
  }
}
