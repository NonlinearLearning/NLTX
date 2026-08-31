using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terraria.Dome.Simulation.Items.Definitions;

namespace Terraria.Dome.Simulation.Items;

public sealed class ItemDefinitionRegistry
{
  private readonly IReadOnlyDictionary<ushort, ItemDefinition> _definitions;
  private readonly IReadOnlyList<ItemDefinition> _orderedDefinitions;

  public ItemDefinitionRegistry(IEnumerable<ItemDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    List<ItemDefinition> materialized = new(definitions);
    Dictionary<ushort, ItemDefinition> indexed = new();
    HashSet<ushort> knownTypes = new();
    foreach (ItemDefinition definition in materialized)
    {
      if (!indexed.TryAdd(definition.ItemType, definition))
      {
        throw new ArgumentException("Item definitions must have unique types.");
      }

      knownTypes.Add(definition.ItemType);
    }

    _definitions = new ReadOnlyDictionary<ushort, ItemDefinition>(indexed);
    _orderedDefinitions = materialized.AsReadOnly();

    foreach (ItemDefinition definition in materialized)
    {
      ItemDefinitionCompiler.Validate(definition, knownTypes);
    }
  }

  public int Count => _definitions.Count;

  public IReadOnlyDictionary<ushort, ItemDefinition> Definitions => _definitions;

  public IReadOnlyList<ItemDefinition> OrderedDefinitions => _orderedDefinitions;

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
