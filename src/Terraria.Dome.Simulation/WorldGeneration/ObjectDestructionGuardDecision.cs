namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct ObjectDestructionGuardDecision(
  bool CanBegin,
  bool SuppressNestedChecks);
