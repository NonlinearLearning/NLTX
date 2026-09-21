namespace Terraria.NpcTownBestiary;

public sealed class BestiaryNpcEntryIndex
{
  private readonly BestiaryCatalogComponent _catalog;

  public BestiaryNpcEntryIndex(BestiaryCatalogComponent catalog)
  {
    _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
  }

  public bool TryGet(
    NpcNetId npcNetId,
    out BestiaryEntryDefinition entry)
  {
    entry = _catalog.FindByNetId(npcNetId)!;
    return entry is not null;
  }
}
