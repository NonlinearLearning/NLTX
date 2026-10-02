namespace NLTX.EconomyCraftingFishingLoot.Fishing;

public static class FishingDepthPredicate
{
  public static bool Height1(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return context.HeightLevel == 1;
  }

  public static bool Height1And2(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return context.HeightLevel == 1 || context.HeightLevel == 2;
  }

  public static bool HeightAbove1(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return context.HeightLevel > 1;
  }

  public static bool HeightAboveAnd1(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return context.HeightLevel >= 1;
  }

  public static bool HeightUnder2(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return context.HeightLevel < 2;
  }

  public static bool HeightAbove2(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return context.HeightLevel > 2;
  }

  public static bool Height0(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return context.HeightLevel == 0;
  }

  public static bool Height2(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return context.HeightLevel == 2;
  }

  public static bool Height3(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return context.HeightLevel == 3;
  }
}
