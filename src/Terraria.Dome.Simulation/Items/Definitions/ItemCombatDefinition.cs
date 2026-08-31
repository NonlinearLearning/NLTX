namespace Terraria.Dome.Simulation.Items.Definitions;

public enum ItemDamageClass : byte
{
  None,
  Melee,
  Ranged,
  Magic,
  Summon,
  Throwing
}

public readonly record struct ItemCombatDefinition(
  int Damage = 0,
  float Knockback = 0,
  int CriticalChance = 0,
  int ArmorPenetration = 0,
  int BonusTagDamage = 0,
  ItemDamageClass DamageClass = ItemDamageClass.None,
  bool Flame = false,
  bool Mech = false,
  ushort ProjectileType = 0,
  float ProjectileSpeed = 0,
  ushort AmmoType = 0,
  bool ConsumesAmmo = false,
  bool NotAmmo = false,
  bool Staff = false,
  bool Claw = false,
  bool NoMelee = false);
