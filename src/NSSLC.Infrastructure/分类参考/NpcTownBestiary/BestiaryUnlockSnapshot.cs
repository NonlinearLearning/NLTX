namespace Terraria.NpcTownBestiary;

public sealed class BestiaryUnlockSnapshot
{
  private BestiaryUnlockSnapshot(
    IReadOnlyDictionary<BestiaryCreditId, int> killCounts,
    IReadOnlySet<BestiaryCreditId> sights,
    IReadOnlySet<BestiaryCreditId> chats)
  {
    KillCounts = killCounts;
    Sights = sights;
    Chats = chats;
  }

  public IReadOnlyDictionary<BestiaryCreditId, int> KillCounts { get; }

  public IReadOnlySet<BestiaryCreditId> Sights { get; }

  public IReadOnlySet<BestiaryCreditId> Chats { get; }

  public static BestiaryUnlockSnapshot Create(
    BestiaryKillCountStateComponent kills,
    BestiarySightDiscoveryStateComponent sights,
    BestiaryChatDiscoveryStateComponent chats)
  {
    Dictionary<BestiaryCreditId, int> killCounts = kills.CountsByCredit.ToDictionary();
    HashSet<BestiaryCreditId> sightValues = new(sights.DiscoveredCredits);
    HashSet<BestiaryCreditId> chatValues = new(chats.DiscoveredCredits);
    return new BestiaryUnlockSnapshot(
      new System.Collections.ObjectModel.ReadOnlyDictionary<BestiaryCreditId, int>(killCounts),
      new ReadOnlySet<BestiaryCreditId>(sightValues),
      new ReadOnlySet<BestiaryCreditId>(chatValues));
  }
}
