namespace Terraria.SpatialMotionPhysics;

public static class MinecartMotionSystem
{
  public static void Apply(
    MinecartMotionState state,
    MinecartTrackSample sample,
    int direction)
  {
    ArgumentNullException.ThrowIfNull(state);
    if (direction == 0)
    {
      throw new ArgumentOutOfRangeException(nameof(direction));
    }

    state.IsOnTrack = true;
    float baseSpeed = sample.TrackType == MinecartTrackType.Booster
      ? 4f
      : 1f;
    float boost = sample.BoostLeft ? 1f : 0f;
    state.Velocity = direction * (baseSpeed + boost);
  }
}
