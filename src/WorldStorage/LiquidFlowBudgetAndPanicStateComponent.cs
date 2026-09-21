using System;

namespace Terraria.WorldStorage;

public sealed class LiquidFlowBudgetAndPanicStateComponent
{
  public LiquidFlowBudgetAndPanicStateComponent(LiquidFlowBudgetPolicy policy)
  {
    ArgumentNullException.ThrowIfNull(policy);
    Reset(policy);
  }

  public int EffectiveMaximumLiquid { get; private set; }

  public int ActiveLiquidCount { get; private set; }

  public int SkipCount { get; private set; }

  public int StuckCount { get; private set; }

  public int StuckAmount { get; private set; }

  public bool IsStuck { get; private set; }

  public bool QuickFall { get; private set; }

  public bool QuickSettle { get; private set; }

  public int WetCounter { get; private set; }

  public int PanicCounter { get; private set; }

  public bool PanicMode { get; private set; }

  public int PanicY { get; private set; }

  public void Reset(LiquidFlowBudgetPolicy policy)
  {
    ArgumentNullException.ThrowIfNull(policy);
    EffectiveMaximumLiquid = policy.EffectiveMaximumLiquid;
    ActiveLiquidCount = 0;
    SkipCount = 0;
    StuckCount = 0;
    StuckAmount = 0;
    IsStuck = false;
    QuickFall = false;
    QuickSettle = false;
    WetCounter = 0;
    PanicCounter = 0;
    PanicMode = false;
    PanicY = 0;
  }

  internal void ApplyTick(
    int activeLiquidCount,
    int skipCount,
    int stuckCount,
    int stuckAmount,
    bool isStuck,
    bool quickFall,
    bool quickSettle,
    int wetCounter,
    int panicCounter,
    bool panicMode,
    int panicY)
  {
    ValidateNonNegative(activeLiquidCount, nameof(activeLiquidCount));
    ValidateNonNegative(skipCount, nameof(skipCount));
    ValidateNonNegative(stuckCount, nameof(stuckCount));
    ValidateNonNegative(stuckAmount, nameof(stuckAmount));
    ValidateNonNegative(wetCounter, nameof(wetCounter));
    ValidateNonNegative(panicCounter, nameof(panicCounter));
    ValidateNonNegative(panicY, nameof(panicY));

    ActiveLiquidCount = activeLiquidCount;
    SkipCount = skipCount;
    StuckCount = stuckCount;
    StuckAmount = stuckAmount;
    IsStuck = isStuck;
    QuickFall = quickFall;
    QuickSettle = quickSettle;
    WetCounter = wetCounter;
    PanicCounter = panicCounter;
    PanicMode = panicMode;
    PanicY = panicY;
  }

  private static void ValidateNonNegative(int value, string parameterName)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
