using System.Collections.ObjectModel;

namespace Terraria.WorldGeneration.Dungeon.Styles;

public sealed class DungeonStyleCatalogSnapshot
{
  public DungeonStyleCatalogSnapshot(
    IReadOnlyList<DungeonStyleMaterialDefinition> entries)
  {
    ArgumentNullException.ThrowIfNull(entries);
    Entries = new ReadOnlyCollection<DungeonStyleMaterialDefinition>(
      entries.ToArray());
  }

  public IReadOnlyList<DungeonStyleMaterialDefinition> Entries { get; }
}
