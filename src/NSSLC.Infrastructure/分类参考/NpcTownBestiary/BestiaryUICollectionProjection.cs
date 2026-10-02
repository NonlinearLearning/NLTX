namespace Terraria.NpcTownBestiary;

public static class BestiaryUICollectionProjection
{
  public static BestiaryUICollectionSnapshot Create(
    BestiaryEntryDefinition entry,
    BestiaryCollectionProviderDefinition provider,
    BestiaryKillCountStateComponent kills,
    BestiarySightDiscoveryStateComponent sights,
    BestiaryChatDiscoveryStateComponent chats,
    int displayIndex)
  {
    ArgumentNullException.ThrowIfNull(entry);
    ArgumentNullException.ThrowIfNull(provider);
    BestiaryEntryUnlockState state = provider.ProviderKind switch
    {
      BestiaryProviderKind.CommonEnemy => BestiaryCollectionUnlockQuery.GetCommonEnemyState(
        kills,
        provider.CreditId,
        new BestiaryCommonEnemyUnlockPolicy(
          provider.QuickUnlock,
          provider.FullKillCountNeeded)),
      BestiaryProviderKind.Critter => BestiaryCollectionUnlockQuery.GetCritterState(
        sights,
        provider.CreditId),
      BestiaryProviderKind.TownNpc => BestiaryCollectionUnlockQuery.GetTownNpcState(
        chats,
        provider.CreditId),
      _ => BestiaryEntryUnlockState.NotKnownAtAll
    };

    return new BestiaryUICollectionSnapshot(entry.Key, state, displayIndex);
  }
}
