namespace Terraria.Dome.Simulation.Npc.Components;

public enum NpcSpawnSource
{
  Natural = 1,
  Statue = 2,
  Event = 3,
  Command = 4
}

public readonly record struct NpcSpawnStateComponent(
  NpcSpawnSource Source,
  float DifficultyScale,
  int ReleaseOwner,
  bool SpawnedFromStatue);
