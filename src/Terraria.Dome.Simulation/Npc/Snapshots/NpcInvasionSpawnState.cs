namespace Terraria.Dome.Simulation.Npc.Snapshots;

public readonly record struct NpcInvasionSpawnState(
  int InvasionType,
  int InvasionSize,
  int InvasionDelayTicks,
  bool ReachedInvasionBossCap = false);
