namespace NLTX.EconomyCraftingFishingLoot.Fishing;

public static class FishingBiomePredicate
{
  public static bool Dungeon(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return context.ZoneDungeon && context.DownedBoss3;
  }

  public static bool Beach(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return context.ZoneBeach;
  }

  public static bool Hallow(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return context.ZoneHallow;
  }

  public static bool GlowingMushrooms(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return context.ZoneGlowshroom;
  }

  public static bool TrueDesert(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return context.ZoneDesert;
  }

  public static bool TrueSnow(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return context.ZoneSnow;
  }

  public static bool Corruption(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return context.RolledCorruption;
  }

  public static bool Crimson(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return context.RolledCrimson;
  }

  public static bool Jungle(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return context.RolledJungle;
  }

  public static bool Snow(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return context.RolledSnow;
  }

  public static bool Desert(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return context.RolledDesert;
  }

  public static bool RolledHallowDesert(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return context.RolledInfectedDesert && context.ZoneHallow;
  }
}
