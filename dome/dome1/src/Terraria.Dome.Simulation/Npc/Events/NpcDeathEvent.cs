using Terraria.Dome.Simulation.Npc;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Npc.Events;

public readonly record struct NpcDeathEvent(
  NpcHandle Npc,
  int LootTableId,
  SimulationVector Position,
  WorldSectionCoordinates Section,
  long Tick);
