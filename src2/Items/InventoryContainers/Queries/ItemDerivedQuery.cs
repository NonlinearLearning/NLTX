namespace Terraria.Items.InventoryContainers;

public static class ItemDerivedQuery
{
  public static ItemDerivedValues Evaluate(
    ItemIdentityAndStackComponent item,
    ItemDefinitionCatalog catalog)
  {
    ArgumentNullException.ThrowIfNull(item);
    ArgumentNullException.ThrowIfNull(catalog);

    if (item.ContentType == 0)
    {
      return new ItemDerivedValues(
        isActive: false,
        isAir: true,
        isCoin: false,
        name: string.Empty,
        originalRarity: 0,
        originalDamage: 0,
        originalDefense: 0,
        value: 0);
    }

    ItemDefinition definition = catalog.GetRequired(item.ContentType);
    bool isAir = item.Stack <= 0;
    bool isCoin = item.ContentType is >= 71 and <= 74;
    string name = item.NameOverride ?? definition.Name;

    return new ItemDerivedValues(
      isActive: true,
      isAir,
      isCoin,
      name,
      definition.Rarity,
      definition.Damage,
      definition.Defense,
      definition.Value);
  }

  public static bool CanStack(ItemStackSnapshot source, ItemStackSnapshot destination)
  {
    ArgumentNullException.ThrowIfNull(source);
    ArgumentNullException.ThrowIfNull(destination);

    return source.ContentType == destination.ContentType
      && source.Prefix == destination.Prefix
      && source.Variant == destination.Variant
      && !source.UniqueStack
      && !destination.UniqueStack
      && source.Stack > 0
      && destination.AvailableCapacity > 0;
  }
}
