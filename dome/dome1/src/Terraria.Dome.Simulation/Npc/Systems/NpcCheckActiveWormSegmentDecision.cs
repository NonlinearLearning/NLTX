using System.Collections.Generic;
using Terraria.Dome.Simulation.Npc.Commands;

namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcCheckActiveWormSegmentDecision(
  bool WasProcessed,
  IReadOnlyList<DespawnNpcCommand> DespawnCommands);
