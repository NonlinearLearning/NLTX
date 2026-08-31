using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Npc.Commands;

public readonly record struct NpcRangedAttackCommand(
  NpcHandle SourceNpc,
  long Sequence,
  int ProjectileType,
  int Damage,
  SimulationVector Position,
  SimulationVector Velocity);
