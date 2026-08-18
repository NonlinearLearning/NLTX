using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Npc.Definitions;

public sealed class NpcDefinitionRegistry
{
  private readonly IReadOnlyDictionary<int, NpcDefinition> _definitions;

  public NpcDefinitionRegistry(IEnumerable<NpcDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    Dictionary<int, NpcDefinition> entries = new();
    foreach (NpcDefinition definition in definitions)
    {
      if (!entries.TryAdd(definition.DefinitionId, definition))
      {
        throw new ArgumentException(
          $"NPC definition ID {definition.DefinitionId} is already registered.",
          nameof(definitions));
      }
    }

    _definitions = entries;
  }

  public int Count => _definitions.Count;

  public bool TryGet(int definitionId, out NpcDefinition definition)
  {
    return _definitions.TryGetValue(definitionId, out definition);
  }

  public NpcDefinition GetRequired(int definitionId)
  {
    if (!_definitions.TryGetValue(definitionId, out NpcDefinition definition))
    {
      throw new KeyNotFoundException($"NPC definition ID {definitionId} is not registered.");
    }

    return definition;
  }
}
