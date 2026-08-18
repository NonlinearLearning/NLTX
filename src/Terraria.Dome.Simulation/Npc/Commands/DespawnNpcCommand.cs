using Terraria.Dome.Simulation.Npc.Components;

namespace Terraria.Dome.Simulation.Npc.Commands;

public readonly record struct DespawnNpcCommand(NpcHandle Npc, NpcDespawnReason Reason);
