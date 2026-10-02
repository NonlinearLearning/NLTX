namespace Terraria.SpatialMotionPhysics;

public static class LiquidContactSystem
{
  public static void Update(
    EntityLiquidContactComponent state,
    LiquidContactSample sample)
  {
    ArgumentNullException.ThrowIfNull(state);
    state.Wet = sample.Wet;
    state.ShimmerWet = sample.ShimmerWet;
    state.HoneyWet = sample.HoneyWet;
    state.WetCount = sample.WetCount;
    state.LavaWet = sample.LavaWet;
  }
}
