using System.Numerics;

namespace Terraria.Player;

public readonly record struct PlayerPoseInputSnapshot(
  SimulationTick Tick,
  bool AdvancePose = true,
  bool ResetPose = false,
  Vector2? HeadVelocityOverride = null,
  Vector2? BodyVelocityOverride = null,
  Vector2? LegVelocityOverride = null,
  float? FullRotation = null,
  Vector2? FullRotationOrigin = null,
  int? FartKartCloudDelay = null,
  float? GfxOffY = null,
  float? StepSpeed = null) : IPlayerPoseInputSnapshot;
