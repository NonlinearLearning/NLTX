using System;
using System.Collections.Generic;

namespace Terraria.Combat;

/// <summary>
/// Registry for immutable definitions; it does not store runtime entity instances.
/// </summary>
public sealed class EntityDefinitionRegistry {
  private readonly Dictionary<EntityDefinitionId, EntityDefinition> _definitions = [];

  public IReadOnlyCollection<EntityDefinition> Definitions => _definitions.Values;

  public void Register(EntityDefinition definition) {
    ArgumentNullException.ThrowIfNull(definition);
    if (!_definitions.TryAdd(definition.Id, definition)) {
      throw new InvalidOperationException(
          $"Entity definition {definition.Id} is already registered.");
    }
  }

  public bool TryGet(EntityDefinitionId id, out EntityDefinition definition) {
    return _definitions.TryGetValue(id, out definition!);
  }

  public EntityDefinition Get(EntityDefinitionId id) {
    if (!TryGet(id, out EntityDefinition definition)) {
      throw new KeyNotFoundException($"Entity definition {id} is not registered.");
    }
    return definition;
  }
}
