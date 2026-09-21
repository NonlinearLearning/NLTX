namespace NLTX.EconomyCraftingFishingLoot.Content;

public sealed class RecipeConditionDefinition
{
  public RecipeConditionDefinition(RecipeConditionKind kind)
  {
    if (kind == RecipeConditionKind.None ||
      !Enum.IsDefined(typeof(RecipeConditionKind), kind))
    {
      throw new ArgumentOutOfRangeException(nameof(kind));
    }

    Kind = kind;
  }

  public RecipeConditionKind Kind { get; }
}
