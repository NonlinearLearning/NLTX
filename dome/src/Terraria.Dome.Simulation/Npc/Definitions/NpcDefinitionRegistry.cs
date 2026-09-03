using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terraria.Dome.Simulation.Npc.Definitions;

public sealed class NpcDefinitionRegistry
{
  private readonly IReadOnlyDictionary<int, NpcDefinition> _definitions;
  private readonly IReadOnlyList<NpcDefinition> _orderedDefinitions;

  public NpcDefinitionRegistry(IEnumerable<NpcDefinition> definitions)
    : this(definitions, targetCapabilities: null)
  {
  }

  public NpcDefinitionRegistry(
    IEnumerable<NpcDefinition> definitions,
    NpcTargetCapabilityRegistry? targetCapabilities)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    List<NpcDefinition> materialized = new(definitions);
    Dictionary<int, NpcDefinition> entries = new();
    for (int index = 0; index < materialized.Count; index++)
    {
      NpcDefinition definition = materialized[index];
      if (targetCapabilities is not null)
      {
        definition = definition.WithSupportsNpcTargets(targetCapabilities.Supports(definition));
        materialized[index] = definition;
      }

      if (!entries.TryAdd(definition.DefinitionId, definition))
      {
        throw new ArgumentException(
          $"NPC definition ID {definition.DefinitionId} is already registered.",
          nameof(definitions));
      }
    }

    _definitions = new ReadOnlyDictionary<int, NpcDefinition>(entries);
    _orderedDefinitions = materialized.AsReadOnly();
  }

  public int Count => _definitions.Count;

  public IReadOnlyDictionary<int, NpcDefinition> Definitions => _definitions;

  public IReadOnlyList<NpcDefinition> OrderedDefinitions => _orderedDefinitions;

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
