namespace Terraria.Share.Attributes;

/// <summary>Behavior categories that remain outside scalar attribute calculation.</summary>
public enum BuffBehaviorKind : byte
{
  DamageOverTime,
  PeriodicHeal,
  Stun,
  Invisibility,
  DamageImmunity,
  MovementRestriction
}
