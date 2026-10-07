using System;

namespace Terraria.WorldStorage;

/// <summary>
/// 保存液体模拟预算、卡住检测和应急流动状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Liquid。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Liquid.cs。</para>
/// <para>
/// 主要源成员：skipCount（第 18 行）； stuckCount（第 20 行）； stuckAmount（第 22 行）； quickFall（第 32 行）；
/// quickSettle（第 34 行）； wetCounter（第 36 行）； panicCounter（第 38 行）； panicMode（第 40 行）； panicY（第 42
/// 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P01-liquid-wiring-spatial-death-teleport-component-design.md。
/// </para>
/// <para>依据位置：第 163 行。</para>
/// </remarks>
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
