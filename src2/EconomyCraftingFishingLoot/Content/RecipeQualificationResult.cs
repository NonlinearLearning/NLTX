namespace NLTX.EconomyCraftingFishingLoot.Content;

public readonly record struct RecipeQualificationResult(
  bool IsQualified,
  RecipeQualificationFailureReason FailureReason,
  int MissingIngredientIndex,
  RecipeConditionKind MissingConditionKind)
{
  public static RecipeQualificationResult Qualified => new(
    IsQualified: true,
    FailureReason: RecipeQualificationFailureReason.None,
    MissingIngredientIndex: -1,
    MissingConditionKind: RecipeConditionKind.None);

  public static RecipeQualificationResult MissingIngredient(int ingredientIndex) => new(
    IsQualified: false,
    RecipeQualificationFailureReason.MissingIngredient,
    ingredientIndex,
    RecipeConditionKind.None);

  public static RecipeQualificationResult MissingRequiredTile => new(
    IsQualified: false,
    RecipeQualificationFailureReason.MissingRequiredTile,
    MissingIngredientIndex: -1,
    RecipeConditionKind.None);

  public static RecipeQualificationResult MissingCondition(RecipeConditionKind kind) => new(
    IsQualified: false,
    RecipeQualificationFailureReason.MissingCondition,
    MissingIngredientIndex: -1,
    kind);
}
