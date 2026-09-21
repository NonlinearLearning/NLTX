namespace NLTX.EconomyCraftingFishingLoot.Content;

public readonly record struct RecipeGroupId(int Value)
{
  public bool IsValid => Value >= 0;
}
