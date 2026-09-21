namespace NLTX.EconomyCraftingFishingLoot.Content;

public sealed class RecipeIngredientDefinition
{
  public RecipeIngredientDefinition(int itemTypeId, int stack)
  {
    if (itemTypeId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(itemTypeId));
    }

    ValidateStack(stack);

    ItemTypeId = itemTypeId;
    RecipeGroupId = null;
    Stack = stack;
  }

  public RecipeIngredientDefinition(RecipeGroupId recipeGroupId, int stack)
  {
    if (!recipeGroupId.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(recipeGroupId));
    }

    ValidateStack(stack);

    ItemTypeId = null;
    RecipeGroupId = recipeGroupId;
    Stack = stack;
  }

  public bool IsRecipeGroup => RecipeGroupId.HasValue;

  public int? ItemTypeId { get; }

  public RecipeGroupId? RecipeGroupId { get; }

  public int Stack { get; }

  private static void ValidateStack(int stack)
  {
    if (stack <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(stack));
    }
  }
}
