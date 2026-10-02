using System.Numerics;

namespace Terraria.Player;

public readonly record struct PlayerPoseSnapshot(
  SimulationTick Tick,
  float HeadRotation,
  float BodyRotation,
  float LegRotation,
  Vector2 HeadPosition,
  Vector2 BodyPosition,
  Vector2 LegPosition,
  Vector2 HeadVelocity,
  Vector2 BodyVelocity,
  Vector2 LegVelocity,
  float FullRotation,
  Vector2 FullRotationOrigin,
  int FartKartCloudDelay,
  float GfxOffY,
  float StepSpeed);
