using System;
using System.Collections.Generic;

namespace Terraria.Item;

/// <summary>
/// Stores immutable item definitions and never stores runtime item entities.
/// </summary>
public sealed class ItemDefinitionRegistry {
  private readonly Dictionary<ItemDefinitionId, ItemDefinition> _definitions = [];

  public IReadOnlyCollection<ItemDefinition> Definitions => _definitions.Values;

  public void Register(ItemDefinition definition) {
    ArgumentNullException.ThrowIfNull(definition);
    if (!_definitions.TryAdd(definition.Id, definition)) {
      throw new InvalidOperationException(
          $"Item definition {definition.Id} is already registered.");
    }
  }

  public bool TryGet(ItemDefinitionId id, out ItemDefinition definition) {
    return _definitions.TryGetValue(id, out definition!);
  }

  public ItemDefinition Get(ItemDefinitionId id) {
    if (!TryGet(id, out ItemDefinition definition)) {
      throw new KeyNotFoundException($"Item definition {id} is not registered.");
    }

    return definition;
  }
}
