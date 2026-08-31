using Terraria.Dome.Simulation.Projectile.Definitions;

namespace Terraria.Dome.Simulation.Combat.Commands;

public enum NpcDamageSourceKind
{
  External = 0,
  HostileNpc = 1,
  Lava = 2
}

public readonly record struct DamageNpcCommand(
  NpcHandle Npc,
  int Amount,
  int SourceIdentity = 0,
  ProjectileDamageClass DamageClass = ProjectileDamageClass.Generic,
  bool IsColdDamage = false,
  NpcDamageSourceKind SourceKind = NpcDamageSourceKind.External,
  NpcHandle SourceNpc = default);
