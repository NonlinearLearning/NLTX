namespace Terraria.Player;

public enum PlayerDamageEligibilityRejectionReason : byte
{
  None,
  EmptySource,
  NonPositiveDamage,
  GeneralImmunity,
  SourceCooldown,
  EmptyEvent,
  InvalidSourceRevision,
  ShadowDodge,
  DuplicateEvent,
}
