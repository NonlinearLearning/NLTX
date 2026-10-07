namespace Terraria.Player.Mount;

public static class MountRuntimeStateQuery
{
  public static MountRuntimeSnapshot Snapshot(
    MountDefinitionCatalog catalog,
    MountRuntimeFrameAndFlightStateComponent frameState,
    MountFatigueAndAbilityStateComponent abilityState,
    MountVariantStateComponent variantState)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    ArgumentNullException.ThrowIfNull(frameState);
    ArgumentNullException.ThrowIfNull(abilityState);
    ArgumentNullException.ThrowIfNull(variantState);

    if (!frameState.IsActive || !frameState.MountType.HasValue ||
      !catalog.TryGet(frameState.MountType.Value, out MountDefinition? definition))
    {
      return new MountRuntimeSnapshot(
        false,
        null,
        frameState.Frame,
        frameState.FrameState,
        frameState.FlightTimeRemainingTicks,
        abilityState.Fatigue,
        abilityState.MaximumFatigue,
        abilityState.IsAbilityCharging,
        abilityState.AbilityCharge,
        abilityState.AbilityCooldownRemainingTicks,
        abilityState.AbilityDurationRemainingTicks,
        abilityState.IsAbilityActive,
        abilityState.IsAimingAbility,
        0f,
        0f,
        0f,
        false,
        false,
        false,
        false,
        false,
        false);
    }

    bool canFly = definition.Movement.FlightTimeMax > 0 ||
      (variantState.Kind == MountVariantStateComponent.MountVariantKind.SelectiveFlying &&
        variantState.AllowedToFly);
    bool canHover = definition.Movement.UsesHover &&
      frameState.FrameState is
        MountRuntimeFrameAndFlightStateComponent.MountFrameStateKind.InAir or
        MountRuntimeFrameAndFlightStateComponent.MountFrameStateKind.Flying or
        MountRuntimeFrameAndFlightStateComponent.MountFrameStateKind.Swimming;
    bool canUseAbility = definition.Ability.ChargeMax > 0 &&
      abilityState.AbilityCooldownRemainingTicks == 0;

    return new MountRuntimeSnapshot(
      true,
      frameState.MountType,
      frameState.Frame,
      frameState.FrameState,
      frameState.FlightTimeRemainingTicks,
      abilityState.Fatigue,
      abilityState.MaximumFatigue,
      abilityState.IsAbilityCharging,
      abilityState.AbilityCharge,
      abilityState.AbilityCooldownRemainingTicks,
      abilityState.AbilityDurationRemainingTicks,
      abilityState.IsAbilityActive,
      abilityState.IsAimingAbility,
      definition.Movement.RunSpeed,
      definition.Movement.DashSpeed,
      definition.Movement.Acceleration,
      definition.Movement.IsMinecart,
      definition.Movement.CanRideMinecartTracks,
      definition.Movement.CanUseWings,
      canFly,
      canHover,
      canUseAbility);
  }
}
