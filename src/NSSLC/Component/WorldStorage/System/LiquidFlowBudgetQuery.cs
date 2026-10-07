using System;

namespace Terraria.WorldStorage;

public static class LiquidFlowBudgetQuery
{
  public static LiquidFlowBudgetDecision Evaluate(
    LiquidFlowBudgetPolicy policy,
    int activeLiquidCount,
    int requestedWorkBudget,
    int bufferedLiquidCount,
    int panicCounter,
    bool panicMode,
    bool quickSettle)
  {
    ArgumentNullException.ThrowIfNull(policy);
    ValidateNonNegative(activeLiquidCount, nameof(activeLiquidCount));
    ValidateNonNegative(requestedWorkBudget, nameof(requestedWorkBudget));
    ValidateNonNegative(bufferedLiquidCount, nameof(bufferedLiquidCount));
    ValidateNonNegative(panicCounter, nameof(panicCounter));

    int availableWorkBudget = CalculateAvailableWorkBudget(
      policy,
      activeLiquidCount,
      requestedWorkBudget);
    int nextPanicCounter = AdvancePanicCounter(
      bufferedLiquidCount,
      panicCounter,
      panicMode);

    return new LiquidFlowBudgetDecision(
      policy.EffectiveMaximumLiquid,
      availableWorkBudget,
      availableWorkBudget > 0 && !panicMode,
      ShouldEnterPanic(bufferedLiquidCount, nextPanicCounter, panicMode),
      nextPanicCounter,
      ShouldUseQuickFall(
        quickSettle,
        policy.UseReducedMaximum,
        activeLiquidCount));
  }

  public static int CalculateAvailableWorkBudget(
    LiquidFlowBudgetPolicy policy,
    int activeLiquidCount,
    int requestedWorkBudget)
  {
    ArgumentNullException.ThrowIfNull(policy);
    ValidateNonNegative(activeLiquidCount, nameof(activeLiquidCount));
    ValidateNonNegative(requestedWorkBudget, nameof(requestedWorkBudget));

    int remainingCapacity = Math.Max(
      0,
      policy.EffectiveMaximumLiquid - activeLiquidCount);
    return Math.Min(requestedWorkBudget, remainingCapacity);
  }

  public static int AdvancePanicCounter(
    int bufferedLiquidCount,
    int panicCounter,
    bool panicMode)
  {
    ValidateNonNegative(bufferedLiquidCount, nameof(bufferedLiquidCount));
    ValidateNonNegative(panicCounter, nameof(panicCounter));
    if (panicMode || bufferedLiquidCount < LiquidFlowBudgetPolicy.PanicBufferHighWaterMark)
    {
      return 0;
    }

    return panicCounter == int.MaxValue
      ? int.MaxValue
      : panicCounter + 1;
  }

  public static bool ShouldEnterPanic(
    int bufferedLiquidCount,
    int panicCounter,
    bool panicMode)
  {
    ValidateNonNegative(bufferedLiquidCount, nameof(bufferedLiquidCount));
    ValidateNonNegative(panicCounter, nameof(panicCounter));
    return !panicMode &&
      bufferedLiquidCount >= LiquidFlowBudgetPolicy.PanicBufferHighWaterMark &&
      panicCounter > LiquidFlowBudgetPolicy.PanicStartAfterTicks;
  }

  public static bool ShouldUseQuickFall(
    bool quickSettle,
    bool useReducedMaximum,
    int activeLiquidCount)
  {
    ValidateNonNegative(activeLiquidCount, nameof(activeLiquidCount));
    return quickSettle ||
      (useReducedMaximum &&
        activeLiquidCount > LiquidFlowBudgetPolicy.ReducedQuickFallLiquidThreshold);
  }

  private static void ValidateNonNegative(int value, string parameterName)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
