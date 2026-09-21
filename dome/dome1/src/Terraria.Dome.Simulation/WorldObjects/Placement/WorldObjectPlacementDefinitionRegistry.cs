using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terraria.Dome.Simulation.WorldObjects.Placement;

public sealed class WorldObjectPlacementDefinitionRegistry
{
  private readonly IReadOnlyDictionary<ushort, WorldObjectPlacementDefinition> _definitions;

  public WorldObjectPlacementDefinitionRegistry(
    IEnumerable<WorldObjectPlacementDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    Dictionary<ushort, WorldObjectPlacementDefinition> indexed = new();
    foreach (WorldObjectPlacementDefinition definition in definitions)
    {
      ArgumentNullException.ThrowIfNull(definition);
      if (!indexed.TryAdd(definition.ObjectType, definition))
      {
        throw new ArgumentException(
          "World-object placement definitions must have unique object types.",
          nameof(definitions));
      }
    }

    _definitions = new ReadOnlyDictionary<ushort, WorldObjectPlacementDefinition>(indexed);
  }

  public IReadOnlyDictionary<ushort, WorldObjectPlacementDefinition> Definitions => _definitions;

  public static WorldObjectPlacementDefinitionRegistry CreateVersion4Base()
  {
    WorldObjectPlacementDefinition sign = new(
      objectType: WorldObjectPlacementRequest.SignObjectType,
      width: 2,
      height: 2,
      originOffsetX: 0,
      originOffsetY: 1,
      coordinateWidth: 16,
      coordinatePadding: 2,
      coordinateHeights: [16, 16],
      styleHorizontal: true,
      styleMultiplier: 1,
      styleWrapLimit: 0,
      randomStyleRange: 0,
      anchorKind: WorldObjectPlacementAnchorKind.Bottom,
      allowedDirections: [-1, 1],
      supportsAlternate: false,
      supportsRandom: false,
      usesCustomCanPlace: true,
      drawYOffset: 2,
      lavaDeath: false);
    return new WorldObjectPlacementDefinitionRegistry([sign]);
  }

  public static WorldObjectPlacementDefinitionRegistry CreateDefault()
  {
    return CreateVersion4Base();
  }

  public bool TryGet(
    ushort objectType,
    out WorldObjectPlacementDefinition definition)
  {
    if (_definitions.TryGetValue(objectType, out WorldObjectPlacementDefinition? value))
    {
      definition = value;
      return true;
    }

    definition = null!;
    return false;
  }
}
