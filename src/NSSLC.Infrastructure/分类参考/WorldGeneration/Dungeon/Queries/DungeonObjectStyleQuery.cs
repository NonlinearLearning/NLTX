using Terraria.WorldGeneration.Dungeon.Catalogs;

namespace Terraria.WorldGeneration.Dungeon.Queries;

public sealed class DungeonObjectStyleQuery
{
  private readonly DungeonObjectStyleCatalog _catalog;

  public DungeonObjectStyleQuery(DungeonObjectStyleCatalog catalog)
  {
    _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
  }

  public bool TrySelect(
    DungeonObjectStyleKey key,
    out int style)
  {
    return _catalog.TryGet(key, out style);
  }
}
