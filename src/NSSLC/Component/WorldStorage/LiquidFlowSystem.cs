using System;

namespace Terraria.WorldStorage;

public sealed class LiquidFlowSystem
{
  private readonly LiquidFlowBudgetPolicy _policy;
  private readonly ILiquidFlowCommitPort _commitPort;

  public LiquidFlowSystem(
    LiquidFlowBudgetPolicy policy,
    ILiquidFlowCommitPort commitPort)
  {
    ArgumentNullException.ThrowIfNull(policy);
    ArgumentNullException.ThrowIfNull(commitPort);
    _policy = policy;
    _commitPort = commitPort;
  }

  public LiquidFlowTickResult Advance(in LiquidFlowTickInput input)
  {
    ValidateInput(input);
    LiquidFlowBudgetDecision decision = LiquidFlowBudgetQuery.Evaluate(
      _policy,
      input.ActiveLiquidCount,
      input.RequestedWorkBudget,
      input.BufferedLiquidCount,
      input.PanicCounter,
      input.PanicMode,
      input.QuickSettle);

    LiquidFlowStateUpdate update = CreateStateUpdate(input, decision);
    LiquidFlowCommitResult commitResult = _commitPort.Commit(in update);
    return new LiquidFlowTickResult(decision, commitResult);
  }

  public LiquidFlowCommitResult Reinitialize()
  {
    return _commitPort.Reset(_policy);
  }

  private static LiquidFlowStateUpdate CreateStateUpdate(
    in LiquidFlowTickInput input,
    in LiquidFlowBudgetDecision decision)
  {
    if (!decision.ShouldEnterPanic)
    {
      return new LiquidFlowStateUpdate(
        input.ActiveLiquidCount,
        input.SkipCount,
        input.StuckCount,
        input.StuckAmount,
        input.IsStuck,
        decision.ShouldUseQuickFall,
        input.QuickSettle,
        input.WetCounter,
        decision.NextPanicCounter,
        input.PanicMode,
        input.PanicY);
    }

    return new LiquidFlowStateUpdate(
      ActiveLiquidCount: 0,
      SkipCount: input.SkipCount,
      StuckCount: input.StuckCount,
      StuckAmount: input.StuckAmount,
      IsStuck: input.IsStuck,
      QuickFall: decision.ShouldUseQuickFall,
      QuickSettle: input.QuickSettle,
      WetCounter: input.WetCounter,
      PanicCounter: 0,
      PanicMode: true,
      PanicY: input.PanicStartY);
  }

  private static void ValidateInput(in LiquidFlowTickInput input)
  {
    ValidateNonNegative(input.ActiveLiquidCount, nameof(input.ActiveLiquidCount));
    ValidateNonNegative(input.RequestedWorkBudget, nameof(input.RequestedWorkBudget));
    ValidateNonNegative(input.BufferedLiquidCount, nameof(input.BufferedLiquidCount));
    ValidateNonNegative(input.SkipCount, nameof(input.SkipCount));
    ValidateNonNegative(input.StuckCount, nameof(input.StuckCount));
    ValidateNonNegative(input.StuckAmount, nameof(input.StuckAmount));
    ValidateNonNegative(input.WetCounter, nameof(input.WetCounter));
    ValidateNonNegative(input.PanicCounter, nameof(input.PanicCounter));
    ValidateNonNegative(input.PanicY, nameof(input.PanicY));
    ValidateNonNegative(input.PanicStartY, nameof(input.PanicStartY));
  }

  private static void ValidateNonNegative(int value, string parameterName)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
