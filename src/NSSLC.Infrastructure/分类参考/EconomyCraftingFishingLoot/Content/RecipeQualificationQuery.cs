namespace NLTX.EconomyCraftingFishingLoot.Content;

public static class RecipeQualificationQuery
{
  public static RecipeQualificationResult Evaluate(
    RecipeDefinition recipe,
    RecipeQualificationContext context,
    RecipeGroupCatalog recipeGroups)
  {
    ArgumentNullException.ThrowIfNull(recipe);
    ArgumentNullException.ThrowIfNull(context);
    ArgumentNullException.ThrowIfNull(recipeGroups);

    for (int index = 0; index < recipe.Ingredients.Length; index++)
    {
      RecipeIngredientDefinition ingredient = recipe.Ingredients[index];
      if (!HasIngredient(context, recipeGroups, ingredient))
      {
        return RecipeQualificationResult.MissingIngredient(index);
      }
    }

    if (recipe.RequiredTileId is int requiredTileId &&
      !context.AvailableTileIds.Contains(requiredTileId))
    {
      return RecipeQualificationResult.MissingRequiredTile;
    }

    foreach (RecipeConditionDefinition condition in recipe.Conditions)
    {
      if (!context.ConditionValues.TryGetValue(condition.Kind, out bool isSatisfied) ||
        !isSatisfied)
      {
        return RecipeQualificationResult.MissingCondition(condition.Kind);
      }
    }

    return RecipeQualificationResult.Qualified;
  }

  private static bool HasIngredient(
    RecipeQualificationContext context,
    RecipeGroupCatalog recipeGroups,
    RecipeIngredientDefinition ingredient)
  {
    if (ingredient.IsRecipeGroup)
    {
      if (!recipeGroups.TryGetByGroupId(
        ingredient.RecipeGroupId!.Value,
        out RecipeGroupDefinition group))
      {
        return false;
      }

      foreach (int itemTypeId in group.ValidItemTypeIds)
      {
        if (context.ItemCounts.TryGetValue(itemTypeId, out int count) &&
          count >= ingredient.Stack)
        {
          return true;
        }
      }

      return false;
    }

    return context.ItemCounts.TryGetValue(ingredient.ItemTypeId!.Value, out int itemCount) &&
      itemCount >= ingredient.Stack;
  }
}
