namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyOceanSandColumnPlan(
  int Iteration,
  int X,
  int Depth,
  int FirstActiveY,
  int CarveDepth,
  bool IsPyramidCandidate,
  int EligibleTileCount,
  int RandomDrawCount);
