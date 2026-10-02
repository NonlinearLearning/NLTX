namespace NLTX.EconomyCraftingFishingLoot.Content;

public sealed class RecipeResultDefinition
{
  public RecipeResultDefinition(int itemTypeId, int stack)
  {
    if (itemTypeId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(itemTypeId));
    }

    if (stack <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(stack));
    }

    ItemTypeId = itemTypeId;
    Stack = stack;
  }

  public int ItemTypeId { get; }

  public int Stack { get; }
}
