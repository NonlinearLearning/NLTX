namespace Terraria.Player;

public readonly record struct PlayerDamageEligibilityInput(
  Guid SourceId,
  int DamageAmount,
  bool HasGeneralImmunity,
  bool SourceCooldownActive,
  bool Dodgeable = true);
