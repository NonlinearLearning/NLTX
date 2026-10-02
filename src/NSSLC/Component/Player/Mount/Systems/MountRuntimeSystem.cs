namespace Terraria.Player.Mount;

public sealed class MountRuntimeSystem
{
  private readonly MountDefinitionCatalog _catalog;

  public MountRuntimeSystem(MountDefinitionCatalog catalog)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    _catalog = catalog;
  }

  public MountActivationResult Activate(
    MountRuntimeFrameAndFlightStateComponent frameState,
    MountFatigueAndAbilityStateComponent abilityState,
    MountVariantStateComponent variantState,
    DrillMountRuntimeComponent drillState,
    ContentId<MountDefinition> mountType)
  {
    ArgumentNullException.ThrowIfNull(frameState);
    ArgumentNullException.ThrowIfNull(abilityState);
    ArgumentNullException.ThrowIfNull(variantState);
    ArgumentNullException.ThrowIfNull(drillState);

    if (!_catalog.TryGet(mountType, out MountDefinition? definition))
    {
      return new MountActivationResult(
        MountActivationResultKind.UnknownDefinition,
        null);
    }

    if (frameState.IsActive && frameState.MountType == mountType)
    {
      return new MountActivationResult(
        MountActivationResultKind.AlreadyActive,
        mountType);
    }

    Reset(
      frameState,
      abilityState,
      variantState,
      drillState);
    frameState.MountType = mountType;
    frameState.IsActive = true;
    frameState.Frame = definition.Animation.Standing.Start;
    frameState.FrameState = MountRuntimeFrameAndFlightStateComponent.MountFrameStateKind.Standing;
    frameState.WalkingGraceRemainingTicks = definition.Movement.WalkingGraceTimeMax;
    abilityState.MaximumFatigue = definition.Movement.FatigueMax;

    return new MountActivationResult(
      MountActivationResultKind.Activated,
      mountType);
  }

  public MountDismountResult Dismount(
    MountRuntimeFrameAndFlightStateComponent frameState,
    MountFatigueAndAbilityStateComponent abilityState,
    MountVariantStateComponent variantState,
    DrillMountRuntimeComponent drillState)
  {
    ArgumentNullException.ThrowIfNull(frameState);
    ArgumentNullException.ThrowIfNull(abilityState);
    ArgumentNullException.ThrowIfNull(variantState);
    ArgumentNullException.ThrowIfNull(drillState);

    ContentId<MountDefinition>? previousMountType = frameState.MountType;
    if (!frameState.IsActive)
    {
      return new MountDismountResult(
        MountDismountResultKind.AlreadyInactive,
        previousMountType);
    }

    Reset(
      frameState,
      abilityState,
      variantState,
      drillState);
    return new MountDismountResult(
      MountDismountResultKind.Dismounted,
      previousMountType);
  }

  public void Reset(
    MountRuntimeFrameAndFlightStateComponent frameState,
    MountFatigueAndAbilityStateComponent abilityState,
    MountVariantStateComponent variantState,
    DrillMountRuntimeComponent drillState)
  {
    ArgumentNullException.ThrowIfNull(frameState);
    ArgumentNullException.ThrowIfNull(abilityState);
    ArgumentNullException.ThrowIfNull(variantState);
    ArgumentNullException.ThrowIfNull(drillState);

    frameState.Reset();
    abilityState.Reset();
    variantState.Reset();
    drillState.Reset();
  }

  public MountFrameUpdateResult UpdateFrame(
    MountRuntimeFrameAndFlightStateComponent frameState,
    in MountFrameUpdateInput input)
  {
    ArgumentNullException.ThrowIfNull(frameState);
    if (!frameState.IsActive || !frameState.MountType.HasValue ||
      !_catalog.TryGet(frameState.MountType.Value, out MountDefinition? definition))
    {
      return new MountFrameUpdateResult(
        false,
        frameState.FrameState,
        frameState.Frame,
        frameState.WalkingGraceRemainingTicks);
    }

    if (frameState.FrameState != input.State)
    {
      frameState.FrameState = input.State;
      frameState.FrameCounter = 0f;
      frameState.IdleTimeTicks = 0;
      frameState.NextIdleTimeTicks = -1;
    }

    if (input.State != MountRuntimeFrameAndFlightStateComponent.MountFrameStateKind.Standing ||
      input.IsDisplayDollOrInanimate)
    {
      frameState.IdleTimeTicks = 0;
      frameState.NextIdleTimeTicks = -1;
    }

    if (input.Velocity.Y == 0f)
    {
      frameState.WalkingGraceRemainingTicks = definition.Movement.WalkingGraceTimeMax;
    }
    else if (frameState.WalkingGraceRemainingTicks > 0)
    {
      frameState.WalkingGraceRemainingTicks--;
    }

    if (input.JustJumped || (input.ControlDown && input.Velocity.Y > 0f))
    {
      frameState.WalkingGraceRemainingTicks = 0;
    }

    MountFrameRange range = definition.Animation.GetRange(input.State);
    if (!range.IsUsable)
    {
      return new MountFrameUpdateResult(
        true,
        input.State,
        frameState.Frame,
        frameState.WalkingGraceRemainingTicks);
    }

    if (!range.Contains(frameState.Frame))
    {
      frameState.Frame = range.Start;
      frameState.FrameCounter = 0f;
    }

    float frameDelta = input.State ==
        MountRuntimeFrameAndFlightStateComponent.MountFrameStateKind.Running ||
      input.State == MountRuntimeFrameAndFlightStateComponent.MountFrameStateKind.Dashing
        ? MathF.Abs(input.Velocity.X)
        : input.State == MountRuntimeFrameAndFlightStateComponent.MountFrameStateKind.Flying
          ? MathF.Max(MathF.Abs(input.Velocity.X), MathF.Abs(input.Velocity.Y))
          : 1f;
    frameState.FrameCounter += frameDelta;
    if (range.Delay > 0 && frameState.FrameCounter > range.Delay)
    {
      frameState.FrameCounter -= range.Delay;
      frameState.Frame = range.Normalize(frameState.Frame + 1);
    }

    return new MountFrameUpdateResult(
      true,
      input.State,
      frameState.Frame,
      frameState.WalkingGraceRemainingTicks);
  }

  public MountResourceTickResult TickResources(
    MountRuntimeFrameAndFlightStateComponent frameState,
    MountFatigueAndAbilityStateComponent abilityState,
    in MountResourceTickInput input)
  {
    ArgumentNullException.ThrowIfNull(frameState);
    ArgumentNullException.ThrowIfNull(abilityState);
    if (!frameState.IsActive || !frameState.MountType.HasValue ||
      !_catalog.TryGet(frameState.MountType.Value, out MountDefinition? definition))
    {
      return ToResourceResult(frameState, abilityState, false);
    }

    bool flightAvailable = true;
    if (input.ConsumeFlightTime)
    {
      flightAvailable = frameState.FlightTimeRemainingTicks > 0;
      if (flightAvailable)
      {
        frameState.FlightTimeRemainingTicks--;
      }
    }

    if (input.RecoverFatigue)
    {
      abilityState.Fatigue = MathF.Max(0f, abilityState.Fatigue - 2f);
    }

    if (input.ChargeAbility)
    {
      abilityState.IsAbilityCharging = true;
    }
    else if (abilityState.IsAbilityCharging)
    {
      abilityState.IsAbilityCharging = false;
      abilityState.AbilityCooldownRemainingTicks = definition.Ability.CooldownTicks;
      abilityState.AbilityDurationRemainingTicks = definition.Ability.DurationTicks;
    }

    if (abilityState.IsAbilityCharging &&
      abilityState.AbilityCharge < definition.Ability.ChargeMax)
    {
      abilityState.AbilityCharge++;
    }
    else if (!abilityState.IsAbilityCharging && abilityState.AbilityCharge > 0)
    {
      abilityState.AbilityCharge--;
    }

    if (abilityState.AbilityCooldownRemainingTicks > 0)
    {
      abilityState.AbilityCooldownRemainingTicks--;
    }

    if (abilityState.AbilityDurationRemainingTicks > 0)
    {
      abilityState.AbilityDurationRemainingTicks--;
    }
    else if (abilityState.IsAbilityActive)
    {
      abilityState.IsAbilityActive = false;
    }

    if (input.SetAbilityActive && input.AbilityActiveValue.HasValue)
    {
      abilityState.IsAbilityActive = input.AbilityActiveValue.Value;
    }

    return ToResourceResult(frameState, abilityState, flightAvailable);
  }

  public void ResetFlightTime(
    MountRuntimeFrameAndFlightStateComponent frameState,
    float horizontalVelocity)
  {
    ArgumentNullException.ThrowIfNull(frameState);
    if (!frameState.IsActive || !frameState.MountType.HasValue ||
      !_catalog.TryGet(frameState.MountType.Value, out MountDefinition? definition))
    {
      frameState.FlightTimeRemainingTicks = 0;
      return;
    }

    int bonus = definition.Id == 0
      ? (int)(MathF.Abs(horizontalVelocity) * 20f)
      : 0;
    frameState.FlightTimeRemainingTicks = definition.Movement.FlightTimeMax + bonus;
  }

  private static MountResourceTickResult ToResourceResult(
    MountRuntimeFrameAndFlightStateComponent frameState,
    MountFatigueAndAbilityStateComponent abilityState,
    bool flightAvailable)
  {
    return new MountResourceTickResult(
      flightAvailable,
      frameState.FlightTimeRemainingTicks,
      abilityState.Fatigue,
      abilityState.AbilityCharge,
      abilityState.AbilityCooldownRemainingTicks,
      abilityState.AbilityDurationRemainingTicks,
      abilityState.IsAbilityActive);
  }
}
