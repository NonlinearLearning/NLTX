namespace Terraria.Items.InventoryContainers;

public static class WorldItemEconomyProjection
{
  public static WorldItemEconomyPayload Create(
    ItemIdentityAndStackComponent item,
    ItemDefinitionCatalog catalog)
  {
    ArgumentNullException.ThrowIfNull(item);
    ArgumentNullException.ThrowIfNull(catalog);
    ItemDerivedValues values = ItemDerivedQuery.Evaluate(item, catalog);
    return new WorldItemEconomyPayload(
      item.ContentType,
      item.Stack,
      item.Favorited,
      values.Value,
      item.MaxStack,
      values.OriginalRarity,
      values.Name,
      values.IsCoin,
      values.IsAir);
  }
}
