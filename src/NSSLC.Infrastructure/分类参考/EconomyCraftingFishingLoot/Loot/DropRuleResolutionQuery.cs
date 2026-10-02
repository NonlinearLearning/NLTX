using NLTX.EconomyCraftingFishingLoot.Content;

namespace NLTX.EconomyCraftingFishingLoot.Loot;

public static class DropRuleResolutionQuery
{
  public static DropAttemptResult EvaluateCommonDrop(
    CommonDropChanceQuantityDefinition definition,
    DropResolutionContext context,
    DropRerollPolicy? rerollPolicy = null,
    DropConditionBranchRuntime? conditionBranch = null)
  {
    ArgumentNullException.ThrowIfNull(definition);
    ArgumentNullException.ThrowIfNull(context);

    if (conditionBranch is not null && !conditionBranch.Matches(context))
    {
      return DropAttemptResult.DoesntFillConditions();
    }

    int totalRolls = rerollPolicy?.TotalRolls ?? 1;
    for (int roll = 1; roll <= totalRolls; roll++)
    {
      int randomValue = context.Random.Next(definition.ChanceDenominator);
      if ((uint)randomValue >= (uint)definition.ChanceDenominator)
      {
        throw new InvalidOperationException(
          "The drop random source returned a value outside the chance range.");
      }

      if (randomValue < definition.ChanceNumerator)
      {
        return DropAttemptResult.Success(
          definition.ItemId,
          definition.AmountDroppedMinimum,
          definition.AmountDroppedMaximum,
          roll);
      }
    }

    return DropAttemptResult.FailedRandomRoll(totalRolls);
  }

  public static DropAttemptResult EvaluateItemOptions(
    DropItemOptionSelector selector,
    DropResolutionContext context)
  {
    ArgumentNullException.ThrowIfNull(selector);
    ArgumentNullException.ThrowIfNull(context);
    if (selector.ItemIds.Length == 0)
    {
      return DropAttemptResult.DidNotRunCode();
    }

    return selector.TrySelect(context, out int itemId)
      ? DropAttemptResult.Success(itemId, 1, 1, 1)
      : DropAttemptResult.FailedRandomRoll(1);
  }

  public static bool TrySelectRuleOption(
    DropRuleOptionSelector selector,
    DropResolutionContext context,
    out DropRuleReference rule)
  {
    ArgumentNullException.ThrowIfNull(selector);
    ArgumentNullException.ThrowIfNull(context);
    return selector.TrySelect(context, out rule);
  }
}
