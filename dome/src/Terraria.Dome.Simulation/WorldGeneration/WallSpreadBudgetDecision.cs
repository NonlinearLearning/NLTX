namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct WallSpreadBudgetDecision(
  int AppliedCount,
  int MaximumCount,
  bool CanSpread,
  bool IsExhausted);
