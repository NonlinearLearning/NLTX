namespace NLTX.EconomyCraftingFishingLoot.Loot;

public sealed class DropChainAttemptContract : DropRuleChainContract
{
  public DropChainAttemptContract(
    Content.DropRuleReference ruleToChain,
    DropChainTriggerKind triggerKind,
    DropChainVisibilityPolicy visibilityPolicy = default)
  {
    RuleToChain = ruleToChain;
    TriggerKind = triggerKind;
    VisibilityPolicy = visibilityPolicy;
  }

  public Content.DropRuleReference RuleToChain { get; }

  public DropChainTriggerKind TriggerKind { get; }

  public DropChainVisibilityPolicy VisibilityPolicy { get; }

  public bool Matches(DropAttemptResult result)
  {
    ArgumentNullException.ThrowIfNull(result);
    return TriggerKind switch
    {
      DropChainTriggerKind.FailedRandomRoll =>
        result.State == DropAttemptResultState.FailedRandomRoll,
      DropChainTriggerKind.Succeeded => result.State == DropAttemptResultState.Success,
      DropChainTriggerKind.DoesntFillConditions =>
        result.State == DropAttemptResultState.DoesntFillConditions,
      DropChainTriggerKind.DidNotRunCode =>
        result.State == DropAttemptResultState.DidNotRunCode,
      _ => false,
    };
  }

  public float GetRateMultiplier(float personalDropRate)
  {
    if (!float.IsFinite(personalDropRate) || personalDropRate < 0 || personalDropRate > 1)
    {
      throw new ArgumentOutOfRangeException(nameof(personalDropRate));
    }

    return TriggerKind switch
    {
      DropChainTriggerKind.FailedRandomRoll => 1 - personalDropRate,
      DropChainTriggerKind.Succeeded => personalDropRate,
      DropChainTriggerKind.DoesntFillConditions => 1,
      DropChainTriggerKind.DidNotRunCode => 1,
      _ => 1,
    };
  }
}
