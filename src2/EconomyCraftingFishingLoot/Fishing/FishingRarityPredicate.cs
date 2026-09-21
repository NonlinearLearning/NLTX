namespace NLTX.EconomyCraftingFishingLoot.Fishing;

public sealed class FishingRarityPredicate
{
  private readonly Func<FishingConditionEvaluationContext, bool> _match;

  public FishingRarityPredicate(
    Func<FishingConditionEvaluationContext, bool> match)
  {
    ArgumentNullException.ThrowIfNull(match);
    _match = match;
  }

  public bool Matches(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return _match.Invoke(context);
  }
}
