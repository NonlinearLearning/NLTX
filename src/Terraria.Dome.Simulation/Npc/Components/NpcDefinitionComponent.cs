using Terraria.Dome.Simulation.Npc.Definitions;

namespace Terraria.Dome.Simulation.Npc.Components;

public readonly record struct NpcDefinitionComponent(
  int DefinitionId,
  int NetId,
  NpcFaction Faction,
  NpcCategory Category);
