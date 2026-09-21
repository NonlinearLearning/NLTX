using System.Numerics;

namespace Terraria.Player;

// Owns the mutable pose state used by the player presentation boundary.
public sealed class PlayerPoseAndAnimationStateComponent
{
  public float HeadRotation { get; private set; }

  public float BodyRotation { get; private set; }

  public float LegRotation { get; private set; }

  public Vector2 HeadPosition { get; private set; }

  public Vector2 BodyPosition { get; private set; }

  public Vector2 LegPosition { get; private set; }

  public Vector2 HeadVelocity { get; private set; }

  public Vector2 BodyVelocity { get; private set; }

  public Vector2 LegVelocity { get; private set; }

  public float FullRotation { get; private set; }

  public Vector2 FullRotationOrigin { get; private set; }

  public int FartKartCloudDelay { get; private set; }

  public float GfxOffY { get; private set; }

  public float StepSpeed { get; private set; } = 1f;

  internal void ApplyInput(IPlayerPoseInputSnapshot input)
  {
    if (input.HeadVelocityOverride.HasValue)
    {
      HeadVelocity = input.HeadVelocityOverride.Value;
    }

    if (input.BodyVelocityOverride.HasValue)
    {
      BodyVelocity = input.BodyVelocityOverride.Value;
    }

    if (input.LegVelocityOverride.HasValue)
    {
      LegVelocity = input.LegVelocityOverride.Value;
    }

    if (input.FullRotation.HasValue)
    {
      FullRotation = input.FullRotation.Value;
    }

    if (input.FullRotationOrigin.HasValue)
    {
      FullRotationOrigin = input.FullRotationOrigin.Value;
    }

    if (input.FartKartCloudDelay.HasValue)
    {
      FartKartCloudDelay = Math.Max(0, input.FartKartCloudDelay.Value);
    }

    if (input.GfxOffY.HasValue)
    {
      GfxOffY = input.GfxOffY.Value;
    }

    if (input.StepSpeed.HasValue)
    {
      StepSpeed = input.StepSpeed.Value;
    }
  }

  internal void AdvancePose()
  {
    HeadPosition += HeadVelocity;
    BodyPosition += BodyVelocity;
    LegPosition += LegVelocity;
    HeadRotation += HeadVelocity.X * 0.1f;
    BodyRotation += BodyVelocity.X * 0.1f;
    LegRotation += LegVelocity.X * 0.1f;
    HeadVelocity = new Vector2(HeadVelocity.X * 0.99f, HeadVelocity.Y + 0.1f);
    BodyVelocity = new Vector2(BodyVelocity.X * 0.99f, BodyVelocity.Y + 0.1f);
    LegVelocity = new Vector2(LegVelocity.X * 0.99f, LegVelocity.Y + 0.1f);
  }

  internal void ResetPose()
  {
    HeadPosition = Vector2.Zero;
    BodyPosition = Vector2.Zero;
    LegPosition = Vector2.Zero;
    HeadRotation = 0f;
    BodyRotation = 0f;
    LegRotation = 0f;
  }

  public PlayerPoseSnapshot ToSnapshot(SimulationTick tick)
  {
    return new PlayerPoseSnapshot(
      tick,
      HeadRotation,
      BodyRotation,
      LegRotation,
      HeadPosition,
      BodyPosition,
      LegPosition,
      HeadVelocity,
      BodyVelocity,
      LegVelocity,
      FullRotation,
      FullRotationOrigin,
      FartKartCloudDelay,
      GfxOffY,
      StepSpeed);
  }
}
