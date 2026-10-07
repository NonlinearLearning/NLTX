namespace Terraria.Player.Mount;

public readonly record struct MountRuntimeMobilityAndAbilitySnapshot(
  bool IsActive,
  ContentId<MountDefinition>? MountType,
  MountEffectiveMovementSnapshot Movement,
  bool IsMinecart,
  bool CanRideMinecartTracks,
  bool CanUseWings,
  bool CanFly,
  bool CanHover,
  bool ConstantJump,
  bool BlockExtraJumps,
  bool DismountsOnItemUse,
  float Fatigue,
  float MaximumFatigue,
  bool IsAbilityCharging,
  int AbilityCharge,
  int AbilityCooldownRemainingTicks,
  int AbilityDurationRemainingTicks,
  bool IsAbilityActive,
  bool IsAimingAbility,
  bool CanUseAbility);
