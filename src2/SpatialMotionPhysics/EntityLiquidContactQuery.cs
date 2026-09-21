namespace Terraria.SpatialMotionPhysics;

public static class EntityLiquidContactQuery
{
  public static bool AnyWet(EntityLiquidContactComponent state)
  {
    ArgumentNullException.ThrowIfNull(state);
    return state.Wet || state.LavaWet || state.HoneyWet || state.ShimmerWet;
  }

  public static bool HasLava(EntityLiquidContactComponent state)
  {
    ArgumentNullException.ThrowIfNull(state);
    return state.LavaWet;
  }
}
