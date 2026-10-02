namespace Terraria.Player;

public readonly record struct PlayerDefenseInteractionResult(
  bool Applied,
  bool ShieldRaised,
  int ShieldParryTimeLeft,
  int ShieldParryCooldown,
  int AttackCooldownFrames,
  bool ResetItemTimers,
  PlayerDefenseInteractionRejectionReason RejectionReason);
