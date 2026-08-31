namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct HousingStinkbugSpawnDecision(
  bool IsBlocked,
  bool HasStinkbug,
  bool HasEchoStinkbug);
