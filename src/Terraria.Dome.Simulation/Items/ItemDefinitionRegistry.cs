using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Items.Definitions;

namespace Terraria.Dome.Simulation.Items;

public sealed class ItemDefinitionRegistry
{
  private readonly Dictionary<ushort, ItemDefinition> _definitions = new();

  public ItemDefinitionRegistry(IEnumerable<ItemDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    List<ItemDefinition> materialized = new(definitions);
    HashSet<ushort> knownTypes = new();
    foreach (ItemDefinition definition in materialized)
    {
      if (!_definitions.TryAdd(definition.ItemType, definition))
      {
        throw new ArgumentException("Item definitions must have unique types.");
      }

      knownTypes.Add(definition.ItemType);
    }

    foreach (ItemDefinition definition in materialized)
    {
      ItemDefinitionCompiler.Validate(definition, knownTypes);
    }
  }

  public int Count => _definitions.Count;

  public ItemDefinition Get(ushort itemType)
  {
    if (!_definitions.TryGetValue(itemType, out ItemDefinition definition))
    {
      throw new ArgumentOutOfRangeException(nameof(itemType));
    }

    return definition;
  }

  public bool TryGet(ushort itemType, out ItemDefinition definition)
  {
    return _definitions.TryGetValue(itemType, out definition);
  }
}
