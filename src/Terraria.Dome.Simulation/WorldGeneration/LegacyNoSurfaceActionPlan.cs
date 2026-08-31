namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyNoSurfaceActionPlan(
  int UndergroundMeteorAttempts,
  bool RandomizeSpawn,
  bool PlaceTorchesAroundSpawn);
