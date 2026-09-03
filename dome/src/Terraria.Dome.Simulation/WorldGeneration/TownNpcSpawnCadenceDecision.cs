namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct TownNpcSpawnCadenceDecision(
  int Delay,
  int Period,
  int NextDelay,
  bool ShouldAttemptSpawn,
  bool IsBlockedByEvent);
