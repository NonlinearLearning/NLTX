using System.Collections.ObjectModel;

namespace Terraria.WorldGeneration.Dungeon.Styles;

public sealed class DungeonStyleCatalog
{
  private readonly IReadOnlyDictionary<DungeonStyleId, DungeonStyleMaterialDefinition> _entries;

  public DungeonStyleCatalog(
    IEnumerable<DungeonStyleMaterialDefinition> entries)
  {
    ArgumentNullException.ThrowIfNull(entries);
    var definitions = entries.ToArray();
    if (definitions.Length == 0)
    {
      throw new ArgumentException("At least one dungeon style is required.", nameof(entries));
    }

    var dictionary = new Dictionary<DungeonStyleId, DungeonStyleMaterialDefinition>();
    foreach (DungeonStyleMaterialDefinition definition in definitions)
    {
      if (!dictionary.TryAdd(definition.Style, definition))
      {
        throw new ArgumentException(
          $"Dungeon style '{definition.Style}' is registered more than once.",
          nameof(entries));
      }
    }

    _entries = new ReadOnlyDictionary<DungeonStyleId, DungeonStyleMaterialDefinition>(dictionary);
  }

  public DungeonStyleMaterialDefinition Get(DungeonStyleId style)
  {
    if (!TryGet(style, out DungeonStyleMaterialDefinition? definition))
    {
      throw new KeyNotFoundException($"Dungeon style '{style}' is not registered.");
    }

    return definition;
  }

  public bool TryGet(
    DungeonStyleId style,
    out DungeonStyleMaterialDefinition definition)
  {
    return _entries.TryGetValue(style, out definition!);
  }

  public DungeonStyleCatalogSnapshot CreateSnapshot()
  {
    return new DungeonStyleCatalogSnapshot(_entries.Values.ToArray());
  }
}
