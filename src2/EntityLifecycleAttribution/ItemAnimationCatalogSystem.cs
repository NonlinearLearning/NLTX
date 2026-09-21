namespace Terraria.EntityLifecycleAttribution;

public sealed class ItemAnimationCatalogSystem
{
  private readonly ItemAnimationCatalog _catalog;

  public ItemAnimationCatalogSystem(ItemAnimationCatalog catalog)
  {
    _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
  }

  public bool Clear()
  {
    bool hadEntries = _catalog.RegisteredContentTypes.Count > 0;
    _catalog.Clear();
    return hadEntries;
  }
}
