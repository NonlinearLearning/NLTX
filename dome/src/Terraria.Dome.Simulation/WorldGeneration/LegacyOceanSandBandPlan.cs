namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyOceanSandBandPlan(
  int Iteration,
  int CandidateX,
  int CandidateRetryCount,
  int Left,
  int RightExclusive,
  int LeftWidth,
  int RightWidth,
  bool IsSkipped,
  int SetupRandomDrawCount);
