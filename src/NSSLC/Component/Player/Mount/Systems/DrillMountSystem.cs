namespace Terraria.Player.Mount;

public sealed class DrillMountSystem
{
  public void Tick(
    DrillMountRuntimeComponent drillState,
    in DrillMountTickInput input)
  {
    ArgumentNullException.ThrowIfNull(drillState);

    drillState.DiodeRotationTarget = input.DiodeRotationTarget;
    drillState.DiodeRotation = MoveTowards(
      drillState.DiodeRotation,
      input.DiodeRotationTarget,
      MathF.Abs(input.RotationStep));
    drillState.OuterRingRotation += input.OuterRingRotationDelta;
    if (drillState.OuterRingRotation > MathF.PI)
    {
      drillState.OuterRingRotation -= MathF.PI * 2f;
    }
    else if (drillState.OuterRingRotation < -MathF.PI)
    {
      drillState.OuterRingRotation += MathF.PI * 2f;
    }

    if (drillState.BeamCooldownTicks > 0)
    {
      drillState.BeamCooldownTicks--;
    }

    DrillMountRuntimeComponent.MountDrillBeamState[] beams =
      drillState.MutableBeamStates;
    for (int i = 0; i < beams.Length; i++)
    {
      DrillMountRuntimeComponent.MountDrillBeamState beam = beams[i];
      if (beam.CooldownTicks > 0)
      {
        int remainingTicks = beam.CooldownTicks - 1;
        beams[i] = remainingTicks == 0
          ? DrillMountRuntimeComponent.MountDrillBeamState.Empty
          : beam with { CooldownTicks = remainingTicks };
      }
    }
  }

  public DrillBeamReservationResult TryReserveBeam(
    DrillMountRuntimeComponent drillState,
    DrillMountRuntimeComponent.DrillTileTarget target,
    DrillMountRuntimeComponent.DrillBeamPurpose purpose,
    int cooldownTicks)
  {
    ArgumentNullException.ThrowIfNull(drillState);
    if (purpose is DrillMountRuntimeComponent.DrillBeamPurpose.Unknown ||
      cooldownTicks <= 0 || drillState.BeamCooldownTicks > 0)
    {
      return new DrillBeamReservationResult(false, -1, null, purpose);
    }

    DrillMountRuntimeComponent.MountDrillBeamState[] beams =
      drillState.MutableBeamStates;
    for (int i = 0; i < beams.Length; i++)
    {
      DrillMountRuntimeComponent.MountDrillBeamState beam = beams[i];
      if (beam.Target == target && beam.Purpose == purpose)
      {
        return new DrillBeamReservationResult(false, -1, null, purpose);
      }
    }

    for (int i = 0; i < beams.Length; i++)
    {
      if (beams[i].CooldownTicks != 0)
      {
        continue;
      }

      beams[i] = new DrillMountRuntimeComponent.MountDrillBeamState(
        target,
        cooldownTicks,
        purpose);
      drillState.BeamCooldownTicks = cooldownTicks;
      return new DrillBeamReservationResult(true, i, target, purpose);
    }

    return new DrillBeamReservationResult(false, -1, null, purpose);
  }

  private static float MoveTowards(float current, float target, float step)
  {
    if (step <= 0f || current == target)
    {
      return current;
    }

    float delta = target - current;
    if (MathF.Abs(delta) <= step)
    {
      return target;
    }

    return current + MathF.Sign(delta) * step;
  }
}
