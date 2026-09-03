namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyErrorWorldSpawnExclusionProfile(
  int WorldWidth,
  double WorldSurface,
  int UnderworldLayer,
  bool IsRemixWorld);
