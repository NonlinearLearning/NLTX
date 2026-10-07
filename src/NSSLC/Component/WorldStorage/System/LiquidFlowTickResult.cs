namespace Terraria.WorldStorage;

public readonly record struct LiquidFlowTickResult(
  LiquidFlowBudgetDecision BudgetDecision,
  LiquidFlowCommitResult CommitResult)
{
  public bool Succeeded => CommitResult.Succeeded;
}
