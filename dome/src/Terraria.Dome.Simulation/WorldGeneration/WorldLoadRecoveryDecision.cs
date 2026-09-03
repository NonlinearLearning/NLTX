namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct WorldLoadRecoveryDecision(
  bool RetryPrimary,
  bool RestoreBackup,
  bool RetryBackup,
  bool LoadSucceeded);
