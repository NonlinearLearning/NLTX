namespace Terraria.NpcTownBestiary;

public sealed class BestiaryCatalogRegistrationSystem
{
  public BestiaryCatalogRegistrationSystem()
  {
    Catalog = new BestiaryCatalogComponent();
  }

  public BestiaryCatalogComponent Catalog { get; }

  public BestiaryEntryDefinition Register(BestiaryEntryDefinition entry)
  {
    return Catalog.Register(entry);
  }

  public void Freeze()
  {
    Catalog.Freeze();
  }

  public bool TryAttachDrop(NpcNetId npcNetId, BestiaryDropRateView dropRate)
  {
    return Catalog.TryAttachDrop(npcNetId, dropRate);
  }
}
