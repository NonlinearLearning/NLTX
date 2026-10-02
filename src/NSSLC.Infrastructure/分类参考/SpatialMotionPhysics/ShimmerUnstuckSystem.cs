namespace Terraria.SpatialMotionPhysics;

public static class ShimmerUnstuckSystem
{
  public static void Start(
    ShimmerUnstuckStateComponent state,
    int duration,
    bool indefinite)
  {
    ArgumentNullException.ThrowIfNull(state);
    if (duration < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(duration));
    }

    state.TimeLeftUnstuck = duration;
    state.IndefiniteProtectionActive = indefinite;
  }

  public static void Tick(ShimmerUnstuckStateComponent state)
  {
    ArgumentNullException.ThrowIfNull(state);
    if (!state.IndefiniteProtectionActive && state.TimeLeftUnstuck > 0)
    {
      state.TimeLeftUnstuck--;
    }
  }

  public static void Clear(ShimmerUnstuckStateComponent state)
  {
    ArgumentNullException.ThrowIfNull(state);
    state.TimeLeftUnstuck = 0;
    state.IndefiniteProtectionActive = false;
  }
}
