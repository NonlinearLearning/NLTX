using System;

namespace Terraria.WorldStorage;

public sealed record LiquidFlowBudgetPolicy
{
  public const int DefaultMaximumBufferLength = 50000;
  public const int DefaultMaximumLiquid = 25000;
  public const int ReducedMaximumLiquid = 5000;
  public const int DefaultCyclesPerUpdate = 10;
  public const int PanicBufferHighWaterMark = 45000;
  public const int PanicStartAfterTicks = 3600;
  public const int ReducedQuickFallLiquidThreshold = 2000;

  public LiquidFlowBudgetPolicy(
    int maximumBufferLength = DefaultMaximumBufferLength,
    int maximumLiquid = DefaultMaximumLiquid,
    int cyclesPerUpdate = DefaultCyclesPerUpdate,
    bool useReducedMaximum = false)
  {
    if (maximumBufferLength <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumBufferLength));
    }

    if (maximumLiquid <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumLiquid));
    }

    if (cyclesPerUpdate <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(cyclesPerUpdate));
    }

    MaximumBufferLength = maximumBufferLength;
    MaximumLiquid = maximumLiquid;
    CyclesPerUpdate = cyclesPerUpdate;
    UseReducedMaximum = useReducedMaximum;
  }

  public int MaximumBufferLength { get; }

  public int MaximumLiquid { get; }

  public int CyclesPerUpdate { get; }

  public bool UseReducedMaximum { get; }

  public int EffectiveMaximumLiquid => UseReducedMaximum
    ? ReducedMaximumLiquid
    : MaximumLiquid;
}
