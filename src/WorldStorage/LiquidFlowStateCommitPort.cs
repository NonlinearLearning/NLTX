using System;

namespace Terraria.WorldStorage;

public sealed class LiquidFlowStateCommitPort : ILiquidFlowCommitPort
{
  private readonly LiquidFlowBudgetAndPanicStateComponent _state;

  public LiquidFlowStateCommitPort(
    LiquidFlowBudgetAndPanicStateComponent state)
  {
    ArgumentNullException.ThrowIfNull(state);
    _state = state;
  }

  public LiquidFlowCommitResult Commit(in LiquidFlowStateUpdate update)
  {
    try
    {
      _state.ApplyTick(
        update.ActiveLiquidCount,
        update.SkipCount,
        update.StuckCount,
        update.StuckAmount,
        update.IsStuck,
        update.QuickFall,
        update.QuickSettle,
        update.WetCounter,
        update.PanicCounter,
        update.PanicMode,
        update.PanicY);
      return LiquidFlowCommitResult.Accepted;
    }
    catch (ArgumentOutOfRangeException exception)
    {
      return LiquidFlowCommitResult.Rejected(exception.ParamName ?? "update");
    }
  }
}
