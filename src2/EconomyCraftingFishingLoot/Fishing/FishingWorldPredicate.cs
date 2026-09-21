namespace NLTX.EconomyCraftingFishingLoot.Fishing;

public static class FishingWorldPredicate
{
  public static bool Remix(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return context.RemixWorld;
  }

  public static bool UnderRockLayer(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return context.RockLayerY is double rockLayerY &&
      context.FishingY >= rockLayerY;
  }

  public static bool OriginalOcean(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return context.IsOriginalOcean == true;
  }

  public static bool RemixOcean(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return context.RolledRemixOcean;
  }

  public static bool Ocean(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return context.RolledRemixOcean || context.IsOriginalOcean == true;
  }

  public static bool Water1000(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return context.WaterTilesCount > 1000;
  }
}
