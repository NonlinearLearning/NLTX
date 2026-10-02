namespace Terraria.WorldStorage;

public sealed class LiquidWorldRuntimeStateComponent
{
  public const int DefaultMaxLiquidBuffer = 50000;
  public const int DefaultMaxLiquid = 25000;
  public const int DefaultCycles = 10;

  public int MaxLiquidBuffer { get; set; } = DefaultMaxLiquidBuffer;
  public int MaxLiquid { get; set; } = DefaultMaxLiquid;
  public int CurrentMaxLiquid { get; set; } = DefaultMaxLiquid;
  public int Cycles { get; set; } = DefaultCycles;
  public int CurrentWorkCount { get; set; }
  public bool IsStuckCleanup { get; set; }
  public bool QuickFall { get; set; }
  public bool QuickSettle { get; set; }
  public int SkipCount { get; set; }
  public int StuckCount { get; set; }
  public int StuckAmount { get; set; }
  public int WetCounter { get; set; }
  public int PanicCounter { get; set; }
  public bool PanicMode { get; set; }
  public int PanicY { get; set; }
}
