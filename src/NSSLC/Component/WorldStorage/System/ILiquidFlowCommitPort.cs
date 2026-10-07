namespace Terraria.WorldStorage;

public interface ILiquidFlowCommitPort
{
  LiquidFlowCommitResult Commit(in LiquidFlowStateUpdate update);

  LiquidFlowCommitResult Reset(LiquidFlowBudgetPolicy policy);
}
