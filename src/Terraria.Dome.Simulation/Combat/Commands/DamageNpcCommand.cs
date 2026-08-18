namespace Terraria.Dome.Simulation.Combat.Commands;

public readonly record struct DamageNpcCommand(
  NpcHandle Npc,
  int Amount,
  int SourceIdentity = 0);
