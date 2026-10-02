namespace NLTX.EconomyCraftingFishingLoot.Content;

public readonly record struct DropRuleReference
{
  public DropRuleReference(string ruleId)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(ruleId);
    RuleId = ruleId;
  }

  public string RuleId { get; }
}
