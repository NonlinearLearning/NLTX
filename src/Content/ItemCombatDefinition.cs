namespace Terraria.Content;

public sealed record ItemCombatDefinition(
  int Damage,
  float KnockBack,
  int CritChance,
  int ArmorPenetration,
  int BonusTagDamage,
  int? ShootTypeId,
  float ShootSpeed,
  int? AmmoTypeId,
  int? UseAmmoTypeId,
  bool IsNotAmmo,
  bool IsMelee,
  bool IsRanged,
  bool IsMagic,
  bool IsSummon,
  bool IsSentry = false);
