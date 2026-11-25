namespace Terraria.Share.Combat;

/// <summary>Stable reason exposed when a damage request does not apply.</summary>
public enum DamageRejectionReason : byte
{
  None = 0,
  InvalidRequest = 1,
  SourceUnavailable = 2,
  SourceInactive = 3,
  TargetUnavailable = 4,
  TargetChanged = 5,
  TargetAlreadyDepleted = 6,
  ProtectionWindowActive = 7,
  RelationRejected = 8,
  NonPositiveDamage = 9,
  NoDamageAfterMitigation = 10,
  DuplicateRequest = 11,
  StaleTick = 12,
  InvalidDamage = 13,
  NoContact = 14,
  TargetImmune = 15,
  /// <summary>The source or target entity does not declare <c>AttributeSetComponent</c>.</summary>
  MissingAttributeCapability = 16
}
