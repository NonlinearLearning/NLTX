namespace Terraria.NpcTownBestiary;

public sealed class BestiaryDropMergeAdapter
{
  public BestiaryDropMergeResult Merge(
    BestiaryCatalogRegistrationSystem catalog,
    IEnumerable<BestiaryDropRegistration> drops)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    ArgumentNullException.ThrowIfNull(drops);
    int appliedCount = 0;
    int missingEntryCount = 0;
    foreach (BestiaryDropRegistration drop in drops)
    {
      if (catalog.TryAttachDrop(drop.NpcNetId, drop.DropRate))
      {
        appliedCount++;
      }
      else
      {
        missingEntryCount++;
      }
    }

    return new BestiaryDropMergeResult(appliedCount, missingEntryCount);
  }
}
