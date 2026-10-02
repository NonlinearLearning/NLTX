namespace Terraria.NpcTownBestiary;

public sealed class BestiaryCollectionProviderDefinition
{
  public BestiaryCollectionProviderDefinition(
    BestiaryProviderKind providerKind,
    BestiaryCreditId creditId,
    bool quickUnlock = false,
    int fullKillCountNeeded = 1)
  {
    if (fullKillCountNeeded <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(fullKillCountNeeded));
    }

    ProviderKind = providerKind;
    CreditId = creditId;
    QuickUnlock = quickUnlock;
    FullKillCountNeeded = fullKillCountNeeded;
  }

  public BestiaryProviderKind ProviderKind { get; }

  public BestiaryCreditId CreditId { get; }

  public bool QuickUnlock { get; }

  public int FullKillCountNeeded { get; }
}
