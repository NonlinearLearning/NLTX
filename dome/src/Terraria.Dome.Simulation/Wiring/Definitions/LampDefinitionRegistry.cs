using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terraria.Dome.Simulation.Wiring.Definitions;

public sealed class LampDefinitionRegistry
{
  private readonly IReadOnlyDictionary<ushort, LampDefinition> _definitions;
  private readonly IReadOnlyList<LampDefinition> _orderedDefinitions;

  public LampDefinitionRegistry(IEnumerable<LampDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    List<LampDefinition> materialized = new(definitions);
    Dictionary<ushort, LampDefinition> indexed = new();
    foreach (LampDefinition definition in materialized)
    {
      ArgumentNullException.ThrowIfNull(definition);
      if (!indexed.TryAdd(definition.TileType, definition))
      {
        throw new ArgumentException("Lamp definitions cannot repeat a Tile type.", nameof(definitions));
      }
    }

    _definitions = new ReadOnlyDictionary<ushort, LampDefinition>(indexed);
    _orderedDefinitions = materialized.AsReadOnly();
  }

  public static LampDefinitionRegistry SourceDerived { get; } = new([
    new LampDefinition(4, 1, 1, 66),
    new LampDefinition(33, 1, 1, 18),
    new LampDefinition(34, 3, 3, 54),
    new LampDefinition(42, 1, 2, 18),
    new LampDefinition(49, 1, 1, 18),
    new LampDefinition(92, 1, 6, 18),
    new LampDefinition(93, 1, 3, 18),
    new LampDefinition(95, 2, 2, 36),
    new LampDefinition(100, 2, 2, 36),
    new LampDefinition(126, 2, 2, 36),
    new LampDefinition(149, 1, 1, 54),
    new LampDefinition(173, 2, 2, 36),
    new LampDefinition(372, 1, 1, 18),
    new LampDefinition(405, 3, 2, 54),
    new LampDefinition(564, 2, 2, 36),
    new LampDefinition(646, 1, 1, 18)
  ]);

  public bool TryGet(ushort tileType, out LampDefinition definition)
  {
    return _definitions.TryGetValue(tileType, out definition!);
  }

  public IReadOnlyDictionary<ushort, LampDefinition> Definitions => _definitions;

  public IReadOnlyList<LampDefinition> OrderedDefinitions => _orderedDefinitions;
}
