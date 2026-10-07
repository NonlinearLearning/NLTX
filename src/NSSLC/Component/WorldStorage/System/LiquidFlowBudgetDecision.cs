namespace Terraria.WorldStorage;

public readonly record struct LiquidFlowBudgetDecision(
  int EffectiveMaximumLiquid,
  int AvailableWorkBudget,
  bool CanConsume,
  bool ShouldEnterPanic,
  int NextPanicCounter,
  bool ShouldUseQuickFall);
