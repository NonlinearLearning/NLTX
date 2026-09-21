namespace Terraria.Player;

public readonly record struct PlayerDamageEligibilityResult(
  bool IsEligible,
  bool UsesShadowDodge,
  PlayerDamageEligibilityRejectionReason RejectionReason);
