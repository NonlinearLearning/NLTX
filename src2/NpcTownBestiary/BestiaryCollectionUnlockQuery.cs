namespace Terraria.NpcTownBestiary;

public static class BestiaryCollectionUnlockQuery
{
  public static BestiaryEntryUnlockState GetCommonEnemyState(
    BestiaryKillCountStateComponent kills,
    BestiaryCreditId creditId,
    BestiaryCommonEnemyUnlockPolicy policy)
  {
    ArgumentNullException.ThrowIfNull(kills);
    int killCount = kills.GetCount(creditId);
    if (policy.QuickUnlock && killCount > 0)
    {
      return BestiaryEntryUnlockState.CanShowDropsWithDropRates;
    }

    if (killCount >= policy.FullKillCountNeeded)
    {
      return BestiaryEntryUnlockState.CanShowDropsWithDropRates;
    }

    if (killCount >= policy.FullKillCountNeeded / 2)
    {
      return BestiaryEntryUnlockState.CanShowDropsWithoutDropRates;
    }

    if (killCount >= policy.FullKillCountNeeded / 5)
    {
      return BestiaryEntryUnlockState.CanShowStats;
    }

    if (killCount >= 1)
    {
      return BestiaryEntryUnlockState.CanShowPortraitOnly;
    }

    return BestiaryEntryUnlockState.NotKnownAtAll;
  }

  public static BestiaryEntryUnlockState GetCritterState(
    BestiarySightDiscoveryStateComponent sights,
    BestiaryCreditId creditId)
  {
    ArgumentNullException.ThrowIfNull(sights);
    return sights.Contains(creditId)
      ? BestiaryEntryUnlockState.CanShowDropsWithDropRates
      : BestiaryEntryUnlockState.NotKnownAtAll;
  }

  public static BestiaryEntryUnlockState GetTownNpcState(
    BestiaryChatDiscoveryStateComponent chats,
    BestiaryCreditId creditId)
  {
    ArgumentNullException.ThrowIfNull(chats);
    return chats.Contains(creditId)
      ? BestiaryEntryUnlockState.CanShowDropsWithDropRates
      : BestiaryEntryUnlockState.NotKnownAtAll;
  }
}
