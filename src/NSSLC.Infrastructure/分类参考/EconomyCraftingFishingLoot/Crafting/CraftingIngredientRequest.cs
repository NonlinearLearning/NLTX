namespace NLTX.EconomyCraftingFishingLoot.Crafting;

public readonly record struct CraftingIngredientRequest
{
  public CraftingIngredientRequest(int itemTypeOrGroupId, int stack = 1)
  {
    if (itemTypeOrGroupId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(itemTypeOrGroupId));
    }

    if (stack <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(stack));
    }

    ItemTypeOrGroupId = itemTypeOrGroupId;
    Stack = stack;
  }

  public int ItemTypeOrGroupId { get; }

  public int Stack { get; }
}
