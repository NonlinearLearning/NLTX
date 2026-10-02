using NLTX.EconomyCraftingFishingLoot.Content;

namespace NLTX.EconomyCraftingFishingLoot.Fishing;

public static class FishingAttemptResolutionQuery
{
  public static FishingResultDecision Resolve(
    FishingDropRuleCatalog catalog,
    FishingAttemptConditionSnapshot attempt,
    IFishingRandomSource random)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    ArgumentNullException.ThrowIfNull(attempt);
    ArgumentNullException.ThrowIfNull(random);

    FishingConditionEvaluationContext context =
      attempt.CreateEvaluationContext(random);
    int rulesEvaluated = 0;
    int randomRollCount = 0;

    foreach (FishingDropRuleDefinition rule in catalog.Rules)
    {
      rulesEvaluated++;
      if (!rule.Rarity.Matches(context) || !rule.ConditionsMatch(context))
      {
        continue;
      }

      if (rule.PossibleItems.IsDefaultOrEmpty)
      {
        return FishingResultDecision.NoResult(
          attempt.RolledEnemySpawn,
          rulesEvaluated,
          randomRollCount,
          stoppedByRule: true);
      }

      int chanceRoll = random.Next(rule.ChanceDenominator);
      randomRollCount++;
      ValidateRandomValue(chanceRoll, rule.ChanceDenominator);
      if (chanceRoll >= rule.ChanceNumerator)
      {
        continue;
      }

      int selectedItemIndex = 0;
      if (rule.PossibleItems.Length > 1)
      {
        selectedItemIndex = random.Next(rule.PossibleItems.Length);
        randomRollCount++;
        ValidateRandomValue(selectedItemIndex, rule.PossibleItems.Length);
      }

      return FishingResultDecision.ItemDrop(
        rule.PossibleItems[selectedItemIndex],
        attempt.RolledEnemySpawn,
        rulesEvaluated,
        randomRollCount);
    }

    return FishingResultDecision.NoResult(
      attempt.RolledEnemySpawn,
      rulesEvaluated,
      randomRollCount);
  }

  private static void ValidateRandomValue(int value, int exclusiveUpperBound)
  {
    if ((uint)value >= (uint)exclusiveUpperBound)
    {
      throw new InvalidOperationException(
        "The fishing random source returned a value outside the requested range.");
    }
  }
}
