namespace Terraria.NpcTownBestiary;

public sealed class BestiaryPlayerJoinSyncAdapter
{
  public BestiaryUnlockSnapshot CreateSnapshot(
    BestiaryKillCountStateComponent kills,
    BestiarySightDiscoveryStateComponent sights,
    BestiaryChatDiscoveryStateComponent chats)
  {
    ArgumentNullException.ThrowIfNull(kills);
    ArgumentNullException.ThrowIfNull(sights);
    ArgumentNullException.ThrowIfNull(chats);
    return BestiaryUnlockSnapshot.Create(kills, sights, chats);
  }
}
