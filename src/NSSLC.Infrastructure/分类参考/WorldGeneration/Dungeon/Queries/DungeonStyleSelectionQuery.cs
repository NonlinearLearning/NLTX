using Terraria.WorldGeneration.Dungeon.Styles;

namespace Terraria.WorldGeneration.Dungeon.Queries;

public sealed class DungeonStyleSelectionQuery
{
  private readonly DungeonStyleCatalog _catalog;

  public DungeonStyleSelectionQuery(DungeonStyleCatalog catalog)
  {
    _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
  }

  public bool TrySelect(
    DungeonStyleId style,
    out DungeonStyleMaterialDefinition definition)
  {
    return _catalog.TryGet(style, out definition!);
  }
}
