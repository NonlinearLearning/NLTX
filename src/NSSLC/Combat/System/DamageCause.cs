namespace Terraria.Combat;

/// <summary>Identifies the domain that produced a damage request.</summary>
public enum DamageCause : byte
{
  Projectile = 0,
  PlayerAttack = 1,
  NpcAttack = 2,
  Environmental = 3
}
