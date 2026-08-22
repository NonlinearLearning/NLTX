namespace Terraria.Dome.Simulation.Npc.Components;

public readonly record struct NpcAuthorityComponent(
  int AiStyle,
  bool IsImmortal,
  bool AlwaysReplicate);
