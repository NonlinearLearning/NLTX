namespace Terraria.NpcTownBestiary;

public sealed class BestiaryEntryDefinition
{
  public BestiaryEntryDefinition(
    BestiaryEntryKey key,
    NpcNetId npcNetId,
    BestiaryCreditId creditId,
    BestiaryProviderKind providerKind,
    string displayName = "")
  {
    Key = key;
    NpcNetId = npcNetId;
    CreditId = creditId;
    ProviderKind = providerKind;
    DisplayName = displayName;
  }

  public BestiaryEntryKey Key { get; }

  public NpcNetId NpcNetId { get; }

  public BestiaryCreditId CreditId { get; }

  public BestiaryProviderKind ProviderKind { get; }

  public int SortingId { get; internal set; }

  public int RarityLevel { get; internal set; }

  public string DisplayName { get; private set; }

  public bool IsBoss { get; private set; }

  public IReadOnlyList<BestiaryInfoElementState> InfoElements { get; internal set; } =
    Array.Empty<BestiaryInfoElementState>();

  public void SetPresentation(int rarityLevel, int sortingId, bool isBoss)
  {
    if (rarityLevel < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(rarityLevel));
    }

    SortingId = sortingId;
    RarityLevel = rarityLevel;
    IsBoss = isBoss;
  }
}
