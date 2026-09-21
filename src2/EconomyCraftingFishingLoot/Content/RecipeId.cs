namespace NLTX.EconomyCraftingFishingLoot.Content;

public readonly record struct RecipeId(int Value)
{
  public bool IsValid => Value >= 0;
}
