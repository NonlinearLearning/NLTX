using Terraria.Dome.Simulation.Projectile.Definitions;

namespace Terraria.Dome.Simulation.Player.Commands;

public readonly record struct DamagePlayerCommand(
  PlayerHandle Player,
  int Amount,
  ProjectileDamageClass DamageClass = ProjectileDamageClass.Generic,
  bool IsColdDamage = false);
