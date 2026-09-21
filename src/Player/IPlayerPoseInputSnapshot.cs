using System.Numerics;

namespace Terraria.Player;

public interface IPlayerPoseInputSnapshot
{
  SimulationTick Tick { get; }

  bool AdvancePose { get; }

  bool ResetPose { get; }

  Vector2? HeadVelocityOverride { get; }

  Vector2? BodyVelocityOverride { get; }

  Vector2? LegVelocityOverride { get; }

  float? FullRotation { get; }

  Vector2? FullRotationOrigin { get; }

  int? FartKartCloudDelay { get; }

  float? GfxOffY { get; }

  float? StepSpeed { get; }
}
