using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terraria.Dome.Simulation.WorldGeneration.Definitions;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed class LegacyErrorWorldTileDefinitionRegistry
{
  private readonly IReadOnlyDictionary<ushort, LegacyErrorWorldTileDefinition> _definitionsByType;

  private LegacyErrorWorldTileDefinitionRegistry(
    IReadOnlyList<LegacyErrorWorldTileDefinition> definitions)
  {
    Definitions = new ReadOnlyCollection<LegacyErrorWorldTileDefinition>([.. definitions]);
    Dictionary<ushort, LegacyErrorWorldTileDefinition> indexed = new(definitions.Count);
    foreach (LegacyErrorWorldTileDefinition definition in definitions)
    {
      if (!indexed.TryAdd(definition.TileType, definition))
      {
        throw new ArgumentException("Tile definitions must contain unique tile IDs.", nameof(definitions));
      }
    }

    _definitionsByType = new ReadOnlyDictionary<ushort, LegacyErrorWorldTileDefinition>(indexed);
  }

  public IReadOnlyList<LegacyErrorWorldTileDefinition> Definitions { get; }

  public IReadOnlyDictionary<ushort, LegacyErrorWorldTileDefinition> ByType => _definitionsByType;

  public static LegacyErrorWorldTileDefinitionRegistry RegisterDefaults()
  {
    TileDefinitionRegistry tileRegistry = TileDefinitionRegistry.RegisterDefaults();
    IReadOnlySet<ushort> dungeonTiles = LegacyEvilReplacementDefinitions.CreateDefault().DungeonTileTypes;
    IReadOnlyList<ushort> frameImportant = TileFrameImportantRegistry.RegisterDefaults();
    HashSet<ushort> frameImportantSet = new(frameImportant);
    List<LegacyErrorWorldTileDefinition> definitions = new(tileRegistry.Definitions.Count);
    foreach (TileDefinition definition in tileRegistry.Definitions)
    {
      definitions.Add(new LegacyErrorWorldTileDefinition(
        definition.TileType,
        IsSolid: definition.BlocksLiquid && !definition.IsPlatform,
        IsSolidTop: definition.IsPlatform,
        IsFrameImportant: frameImportantSet.Contains(definition.TileType),
        IsDungeon: dungeonTiles.Contains(definition.TileType)));
    }

    return new LegacyErrorWorldTileDefinitionRegistry(definitions);
  }
}
