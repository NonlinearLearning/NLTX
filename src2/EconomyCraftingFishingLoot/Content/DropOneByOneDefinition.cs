namespace NLTX.EconomyCraftingFishingLoot.Content;

public sealed class DropOneByOneDefinition
{
  public DropOneByOneDefinition(int itemId, DropOneByOneParameters parameters)
  {
    if (itemId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(itemId));
    }

    ItemId = itemId;
    Parameters = parameters;
  }

  public int ItemId { get; }

  public DropOneByOneParameters Parameters { get; }
}
