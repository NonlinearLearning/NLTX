namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct AltarBreakProgression(
  int PreviousCount,
  int NextCount,
  int CycleIndex,
  int CycleNumber);
