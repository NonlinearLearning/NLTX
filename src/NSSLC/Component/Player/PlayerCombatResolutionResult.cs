namespace Terraria.Player;

public readonly record struct PlayerCombatResolutionResult(
  bool Applied,
  bool Dodged,
  int FinalDamage,
  int LifeAfter,
  bool Killed,
  bool ProcPublished,
  bool TelemetryPublished,
  PlayerDamageEligibilityRejectionReason RejectionReason)
{
  public static PlayerCombatResolutionResult Rejected(
    PlayerDamageEligibilityRejectionReason reason,
    int life)
  {
    return new PlayerCombatResolutionResult(
      Applied: false,
      Dodged: reason == PlayerDamageEligibilityRejectionReason.ShadowDodge,
      FinalDamage: 0,
      LifeAfter: life,
      Killed: life <= 0,
      ProcPublished: false,
      TelemetryPublished: false,
      RejectionReason: reason);
  }
}
