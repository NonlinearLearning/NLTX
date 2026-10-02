namespace NLTX.EconomyCraftingFishingLoot.Fishing;

public static class FishingEventPredicate
{
  public static bool BloodMoon(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return context.BloodMoon;
  }
}
