namespace Terraria.Items.InventoryContainers;

public sealed class ItemDefinitionCatalog
{
  private readonly Dictionary<int, ItemDefinition> _definitions = new();

  public int Count => _definitions.Count;

  public void Register(ItemDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(definition);

    if (!_definitions.TryAdd(definition.Type, definition))
    {
      throw new InvalidOperationException(
        $"An item definition for type {definition.Type} is already registered.");
    }
  }

  public bool TryGet(int type, out ItemDefinition? definition)
  {
    return _definitions.TryGetValue(type, out definition);
  }

  public ItemDefinition GetRequired(int type)
  {
    if (!_definitions.TryGetValue(type, out ItemDefinition? definition))
    {
      throw new KeyNotFoundException($"No item definition exists for type {type}.");
    }

    return definition;
  }
}
