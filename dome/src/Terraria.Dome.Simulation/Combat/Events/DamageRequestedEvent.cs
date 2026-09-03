using Arch.Core;
using Terraria.Dome.Simulation.Projectile.Definitions;

namespace Terraria.Dome.Simulation.Combat.Events;

public readonly record struct DamageRequestedEvent(
  Entity Projectile,
  Entity Target,
  int Amount,
  int ProjectileIdentity,
  int TargetIdentity,
  ProjectileDamageClass DamageClass = ProjectileDamageClass.Generic,
  bool IsColdDamage = false,
  int ArmorPenetration = 0,
  int BonusCritChance = 0,
  int BonusTagDamage = 0,
  int TagEffectType = 0);
