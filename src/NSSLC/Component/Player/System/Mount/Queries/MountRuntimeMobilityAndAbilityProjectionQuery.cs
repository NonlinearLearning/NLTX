namespace Terraria.Player.Mount;

public static class MountRuntimeMobilityAndAbilityProjectionQuery
{
  public static MountRuntimeMobilityAndAbilitySnapshot Snapshot(
    MountDefinitionCatalog catalog,
    MountRuntimeFrameAndFlightStateComponent frameState,
    MountFatigueAndAbilityStateComponent abilityState,
    MountVariantStateComponent variantState,
    bool isUsingSuperCart)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    ArgumentNullException.ThrowIfNull(frameState);
    ArgumentNullException.ThrowIfNull(abilityState);
    ArgumentNullException.ThrowIfNull(variantState);

    if (!frameState.IsActive || !frameState.MountType.HasValue ||
      !catalog.TryGet(frameState.MountType.Value, out MountDefinition? definition))
    {
      return Empty(abilityState);
    }

    MountMovementDefinition movement = definition.Movement;
    MountEffectiveMovementSnapshot effectiveMovement = MountSuperCartQuery.Project(
      movement,
      isUsingSuperCart && movement.IsMinecart);
    bool selectiveFlying =
      variantState.Kind == MountVariantStateComponent.MountVariantKind.SelectiveFlying &&
      variantState.AllowedToFly;
    bool canFly = definition.Id != 48 &&
      (movement.FlightTimeMax > 0 || selectiveFlying);
    bool canHover = movement.UsesHover &&
      (definition.Id != 49 ||
        frameState.FrameState == MountRuntimeFrameAndFlightStateComponent.MountFrameStateKind.Swimming);
    bool canUseAbility = definition.Ability.ChargeMax > 0 &&
      abilityState.AbilityCooldownRemainingTicks == 0;

    return new MountRuntimeMobilityAndAbilitySnapshot(
      true,
      frameState.MountType,
      effectiveMovement,
      movement.IsMinecart,
      movement.CanRideMinecartTracks,
      movement.CanUseWings,
      canFly,
      canHover,
      movement.ConstantJump,
      movement.BlockExtraJumps,
      movement.DismountsOnItemUse,
      abilityState.Fatigue,
      abilityState.MaximumFatigue,
      abilityState.IsAbilityCharging,
      abilityState.AbilityCharge,
      abilityState.AbilityCooldownRemainingTicks,
      abilityState.AbilityDurationRemainingTicks,
      abilityState.IsAbilityActive,
      abilityState.IsAimingAbility,
      canUseAbility);
  }

  private static MountRuntimeMobilityAndAbilitySnapshot Empty(
    MountFatigueAndAbilityStateComponent abilityState)
  {
    return new MountRuntimeMobilityAndAbilitySnapshot(
      false,
      null,
      default,
      false,
      false,
      false,
      false,
      false,
      false,
      false,
      false,
      abilityState.Fatigue,
      abilityState.MaximumFatigue,
      abilityState.IsAbilityCharging,
      abilityState.AbilityCharge,
      abilityState.AbilityCooldownRemainingTicks,
      abilityState.AbilityDurationRemainingTicks,
      abilityState.IsAbilityActive,
      abilityState.IsAimingAbility,
      false);
  }
}
