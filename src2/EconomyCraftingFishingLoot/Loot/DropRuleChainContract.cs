namespace NLTX.EconomyCraftingFishingLoot.Loot;

public interface DropRuleChainContract
{
  Content.DropRuleReference RuleToChain { get; }

  DropChainTriggerKind TriggerKind { get; }

  DropChainVisibilityPolicy VisibilityPolicy { get; }

  bool Matches(DropAttemptResult result);
}
