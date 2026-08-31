namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct FossilBreakDecision(
  bool CanBegin,
  int RollExclusiveUpperBound);
