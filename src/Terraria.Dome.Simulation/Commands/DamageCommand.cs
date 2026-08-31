using Arch.Core;
using Terraria.Dome.Simulation.Projectile.Definitions;

namespace Terraria.Dome.Simulation.Commands;

public readonly record struct DamageCommand(
  Entity Source,
  Entity Target,
  int Amount,
  ProjectileDamageClass DamageClass = ProjectileDamageClass.Generic,
  bool IsColdDamage = false,
  int ArmorPenetration = 0,
  int BonusCritChance = 0,
  int BonusTagDamage = 0,
  int TagEffectType = 0);
