namespace Terraria.SpatialMotionPhysics;

public static class ShimmerUnstuckQuery
{
  public static bool ShouldUnstuck(ShimmerUnstuckStateComponent state)
  {
    ArgumentNullException.ThrowIfNull(state);
    return state.IndefiniteProtectionActive || state.TimeLeftUnstuck > 0;
  }
}
