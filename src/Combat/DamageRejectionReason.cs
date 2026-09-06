namespace Terraria.Combat;

public enum DamageRejectionReason : byte
{
  None,
  NonPositiveAmount,
  GeneralImmunity,
  SourceTargetCooldown,
}
