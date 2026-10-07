namespace Terraria.WorldStorage;

public readonly record struct LiquidWorkItemReadiness(
  bool IsReady,
  bool ShouldRemove,
  bool ShouldDecrementDelay);
