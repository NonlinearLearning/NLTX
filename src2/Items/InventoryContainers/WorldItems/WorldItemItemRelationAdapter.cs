namespace Terraria.Items.InventoryContainers;

public sealed class WorldItemItemRelationAdapter
{
  public WorldItemItemRelationAdapter(ItemIdentityAndStackComponent item)
  {
    Item = item ?? throw new ArgumentNullException(nameof(item));
  }

  public ItemIdentityAndStackComponent Item { get; }
}
