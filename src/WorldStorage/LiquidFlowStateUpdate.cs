namespace Terraria.WorldStorage;

public readonly record struct LiquidFlowStateUpdate(
  int ActiveLiquidCount,
  int SkipCount,
  int StuckCount,
  int StuckAmount,
  bool IsStuck,
  bool QuickFall,
  bool QuickSettle,
  int WetCounter,
  int PanicCounter,
  bool PanicMode,
  int PanicY);
