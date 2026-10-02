namespace Terraria.NpcTownBestiary;

public sealed class BestiaryDiscoveryNetworkAdapter
{
  public BestiaryDiscoveryNetworkPayload CreateKillPayload(
    NpcNetId npcNetId,
    int killCount)
  {
    return new BestiaryDiscoveryNetworkPayload(
      BestiaryDiscoveryEventKind.Kill,
      npcNetId,
      BestiaryKillCountPolicy.Clamp(killCount));
  }

  public BestiaryDiscoveryNetworkPayload CreateSightPayload(NpcNetId npcNetId)
  {
    return new BestiaryDiscoveryNetworkPayload(
      BestiaryDiscoveryEventKind.Sight,
      npcNetId,
      0);
  }

  public BestiaryDiscoveryNetworkPayload CreateChatPayload(NpcNetId npcNetId)
  {
    return new BestiaryDiscoveryNetworkPayload(
      BestiaryDiscoveryEventKind.Chat,
      npcNetId,
      0);
  }

  public BestiaryDiscoveryMutationResult Apply(
    BestiaryDiscoveryNetworkPayload payload,
    BestiaryCreditId creditId,
    BestiaryDiscoverySystem discovery,
    BestiaryKillCountStateComponent kills,
    BestiarySightDiscoveryStateComponent sights,
    BestiaryChatDiscoveryStateComponent chats)
  {
    ArgumentNullException.ThrowIfNull(discovery);
    ArgumentNullException.ThrowIfNull(kills);
    ArgumentNullException.ThrowIfNull(sights);
    ArgumentNullException.ThrowIfNull(chats);

    return payload.Kind switch
    {
      BestiaryDiscoveryEventKind.Kill => discovery.SetKillCount(
        kills,
        creditId,
        payload.KillCount),
      BestiaryDiscoveryEventKind.Sight => discovery.RegisterSight(sights, creditId),
      BestiaryDiscoveryEventKind.Chat => discovery.RegisterChat(chats, creditId),
      _ => new BestiaryDiscoveryMutationResult(false, false, 0)
    };
  }
}
