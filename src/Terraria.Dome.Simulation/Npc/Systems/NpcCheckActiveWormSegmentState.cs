namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcCheckActiveWormSegmentState(
  NpcHandle Handle,
  int AiStyle,
  float NextSegment,
  bool IsActive);
