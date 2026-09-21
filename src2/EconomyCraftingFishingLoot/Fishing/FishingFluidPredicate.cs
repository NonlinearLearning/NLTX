namespace NLTX.EconomyCraftingFishingLoot.Fishing;

public static class FishingFluidPredicate
{
  public static bool InLava(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return context.InLava;
  }

  public static bool InHoney(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return context.InHoney;
  }

  public static bool CanFishInLava(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return context.CanFishInLava;
  }
}
