namespace NLTX.EconomyCraftingFishingLoot.Crafting;

public readonly record struct CraftingItemSnapshot
{
  public CraftingItemSnapshot(int itemTypeId, int prefixId, int stack)
  {
    if (itemTypeId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(itemTypeId));
    }

    if (prefixId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(prefixId));
    }

    if (stack < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(stack));
    }

    ItemTypeId = itemTypeId;
    PrefixId = prefixId;
    Stack = stack;
  }

  public int ItemTypeId { get; }

  public int PrefixId { get; }

  public int Stack { get; }
}
