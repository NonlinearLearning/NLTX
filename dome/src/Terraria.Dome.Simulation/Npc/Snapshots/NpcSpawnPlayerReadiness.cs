namespace Terraria.Dome.Simulation.Npc.Snapshots;

public readonly record struct NpcSpawnPlayerReadiness(
  bool IsActive,
  bool IsDead,
  bool IsJourneyMode,
  bool IsSpawnRateDisabled,
  bool IsNearMoonLord);
