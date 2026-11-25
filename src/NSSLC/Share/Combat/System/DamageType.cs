namespace Terraria.Share.Combat;

/// <summary>Describes the calculation family of one damage application.</summary>
public enum DamageType : byte
{
  Generic = 0,
  Melee = 1,
  Ranged = 2,
  Magic = 3,
  Summon = 4,
  Environmental = 5,
  True = 6,
  DamageOverTime = 7
}
