using NLTX.EconomyCraftingFishingLoot.Content;

namespace NLTX.EconomyCraftingFishingLoot.Loot;

public sealed class DropConditionBranchRuntime
{
  public DropConditionBranchRuntime(
    DropConditionBranchKind kind,
    DropConditionDefinition? condition = null)
  {
    if (kind == DropConditionBranchKind.ItemDropWithCondition && condition is null)
    {
      throw new ArgumentNullException(nameof(condition));
    }

    Kind = kind;
    Condition = condition;
    RequiresIntegrationOwnedCondition =
      kind == DropConditionBranchKind.IntegrationOwned ||
      condition?.Kind == DropConditionKind.IntegrationOwned;
    IsEvaluable = condition is not null && !RequiresIntegrationOwnedCondition;
  }

  public DropConditionBranchKind Kind { get; }

  public DropConditionDefinition? Condition { get; }

  public bool RequiresIntegrationOwnedCondition { get; }

  public bool IsEvaluable { get; }

  public static DropConditionBranchRuntime MechanicalBossesDummy()
  {
    return new DropConditionBranchRuntime(
      DropConditionBranchKind.IntegrationOwned,
      DropConditionDefinition.IntegrationOwned("mechanical-bosses"));
  }

  public bool Matches(DropResolutionContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return IsEvaluable && Condition!.Matches(context);
  }
}
