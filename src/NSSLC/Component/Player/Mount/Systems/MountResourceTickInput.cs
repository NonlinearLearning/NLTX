namespace Terraria.Player.Mount;

public readonly record struct MountResourceTickInput(
  bool RecoverFatigue,
  bool ConsumeFlightTime,
  bool ChargeAbility,
  bool SetAbilityActive,
  bool? AbilityActiveValue);

public readonly record struct MountResourceTickResult(
  bool FlightAvailable,
  int FlightTimeRemainingTicks,
  float Fatigue,
  int AbilityCharge,
  int AbilityCooldownRemainingTicks,
  int AbilityDurationRemainingTicks,
  bool IsAbilityActive);
