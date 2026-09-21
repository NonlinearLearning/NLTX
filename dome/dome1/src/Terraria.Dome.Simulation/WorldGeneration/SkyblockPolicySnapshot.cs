namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct SkyblockPolicySnapshot(
  bool DenyFloatingIslands,
  bool DenyAllGeneration,
  bool DenySomeGeneration,
  bool SpawnSolidifier,
  bool SpawnShimmerPool);
