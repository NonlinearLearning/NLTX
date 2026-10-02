namespace Terraria.WorldStorage;

public sealed class LiquidSchedulerState
{
  public const int DefaultCyclesPerTick = 10;
  public const int DefaultMaximumActiveItems = 25000;

  public int ConfiguredMaximumActiveItems = DefaultMaximumActiveItems;
  public int ConfiguredMaxActiveItems => ConfiguredMaximumActiveItems;
  public int CurrentMaximumActiveItems = DefaultMaximumActiveItems;
  public int CurrentMaxActiveItems => CurrentMaximumActiveItems;
  public int CurrentWorkBudget;
  public int CyclesPerTick = DefaultCyclesPerTick;
  public bool IsQuickFallEnabled;
  public bool IsQuickSettleEnabled;
  public bool IsStuck;
  public int SkipCount;
  public int StuckAmount;
  public int StuckCount;
}
