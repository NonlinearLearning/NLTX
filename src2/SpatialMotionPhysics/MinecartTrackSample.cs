namespace Terraria.SpatialMotionPhysics;

public readonly record struct MinecartTrackSample(
  MinecartTrackType TrackType,
  bool BoostLeft,
  int Frame);
