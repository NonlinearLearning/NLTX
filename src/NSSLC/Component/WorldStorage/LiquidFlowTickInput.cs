namespace Terraria.WorldStorage;

public readonly record struct LiquidFlowTickInput(
  int ActiveLiquidCount,
  int RequestedWorkBudget,
  int BufferedLiquidCount,
  int SkipCount,
  int StuckCount,
  int StuckAmount,
  bool IsStuck,
  bool QuickSettle,
  int WetCounter,
  int PanicCounter,
  bool PanicMode,
  int PanicY,
  int PanicStartY);
