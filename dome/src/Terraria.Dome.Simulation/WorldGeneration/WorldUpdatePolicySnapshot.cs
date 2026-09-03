namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct WorldUpdatePolicySnapshot(
  bool HardModeWorldUpdates,
  bool AllowedToSpreadInfections,
  bool GrowGrassUnderground);
